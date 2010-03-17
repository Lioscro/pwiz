/*
 * Original author: Brendan MacLean <brendanx .at. u.washington.edu>,
 *                  MacCoss Lab, Department of Genome Sciences, UW
 *
 * Copyright 2009 University of Washington - Seattle, WA
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using pwiz.Skyline.Controls.Graphs;
using pwiz.Skyline.Model.DocSettings;

namespace pwiz.Skyline.Model
{
    public sealed class RefinementSettings
    {
        private bool _removeDuplicatePeptides;

        public int? MinPeptidesPerProtein { get; set; }
        public bool RemoveDuplicatePeptides
        {
            get { return _removeDuplicatePeptides; }
            set
            {
                _removeDuplicatePeptides = value;
                // Removing duplicate peptides implies removing
                // repeated peptids.
                if (_removeDuplicatePeptides)
                    RemoveRepeatedPeptides = true;
            }
        }
        public bool RemoveRepeatedPeptides { get; set; }
        public int? MinTransitionsPepPrecursor { get; set; }
        public IsotopeLabelType? RefineLabelType { get; set; }
        public bool AddLabelType { get; set; }
        public double? MinPeakFoundRatio { get; set; }
        public double? MaxPeakFoundRatio { get; set; }
        public double? MaxPeakRank { get; set; }
        public bool RemoveMissingResults { get; set; }
        public double? RTRegressionThreshold { get; set; }
        public double? DotProductThreshold { get; set; }

        public SrmDocument Refine(SrmDocument document)
        {
            HashSet<int> outlierIds = new HashSet<int>();
            if (RTRegressionThreshold.HasValue)
            {
                var outliers = RTLinearRegressionGraphPane.CalcOutliers(document,
                    RTRegressionThreshold.Value);

                foreach (var nodePep in outliers)
                    outlierIds.Add(nodePep.Id.GlobalIndex);
            }

            HashSet<string> includedPeptides = (RemoveRepeatedPeptides ? new HashSet<string>() : null);
            HashSet<string> repeatedPeptides = (RemoveDuplicatePeptides ? new HashSet<string>() : null);

            var listPepGroups = new List<PeptideGroupDocNode>();
            // Excluding proteins with too few peptides, since they can impact results
            // of the duplicate peptide check.
            int minPeptides = MinPeptidesPerProtein ?? 0;
            foreach (PeptideGroupDocNode nodePepGroup in document.Children)
            {
                PeptideGroupDocNode nodePepGroupRefined =
                    Refine(nodePepGroup, document, outlierIds, includedPeptides, repeatedPeptides);

                if (nodePepGroupRefined.Children.Count < minPeptides)
                    continue;

                listPepGroups.Add(nodePepGroupRefined);
            }

            // Need a second pass, if all duplicate peptides should be removed,
            // and duplicates were found.
            if (repeatedPeptides != null && repeatedPeptides.Count > 0)
            {
                var listPepGroupsFiltered = new List<PeptideGroupDocNode>();
                foreach (PeptideGroupDocNode nodePepGroup in listPepGroups)
                {
                    var listPeptides = new List<PeptideDocNode>();
                    foreach (PeptideDocNode nodePep in nodePepGroup.Children)
                    {
                        string pepModSeq = document.Settings.GetModifiedSequence(nodePep.Peptide.Sequence,
                            IsotopeLabelType.light, nodePep.ExplicitMods);
                        if (!repeatedPeptides.Contains(pepModSeq))
                            listPeptides.Add(nodePep);
                    }

                    PeptideGroupDocNode nodePepGroupRefined = (PeptideGroupDocNode)
                        nodePepGroup.ChangeChildrenChecked(listPeptides.ToArray(), true);

                    if (nodePepGroupRefined.Children.Count < minPeptides)
                        continue;

                    listPepGroupsFiltered.Add(nodePepGroupRefined);
                }

                listPepGroups = listPepGroupsFiltered;                
            }

            return (SrmDocument) document.ChangeChildrenChecked(listPepGroups.ToArray(), true);
        }

// ReSharper disable SuggestBaseTypeForParameter
        private PeptideGroupDocNode Refine(PeptideGroupDocNode nodePepGroup,
// ReSharper restore SuggestBaseTypeForParameter
                                           SrmDocument document,
                                           ICollection<int> outlierIds,
                                           ICollection<string> includedPeptides,
                                           ICollection<string> repeatedPeptides)
        {
            var listPeptides = new List<PeptideDocNode>();
            foreach (PeptideDocNode nodePep in nodePepGroup.Children)
            {
                if (outlierIds.Contains(nodePep.Id.GlobalIndex))
                    continue;

                float? peakFoundRatio = nodePep.AveragePeakCountRatio;
                if (!peakFoundRatio.HasValue)
                {
                    if (RemoveMissingResults)
                        continue;
                }
                else
                {
                    if (MinPeakFoundRatio.HasValue)
                    {
                        if (peakFoundRatio < MinPeakFoundRatio.Value)
                            continue;
                    }
                    if (MaxPeakFoundRatio.HasValue)
                    {
                        if (peakFoundRatio > MaxPeakFoundRatio.Value)
                            continue;
                    }
                }

                PeptideDocNode nodePepRefined = Refine(nodePep, document);
                // Always remove peptides if all precursors have been removed by refinement
                if (!ReferenceEquals(nodePep, nodePepRefined) && nodePepRefined.Children.Count == 0)
                    continue;

                if (includedPeptides != null)
                {
                    string pepModSeq = document.Settings.GetModifiedSequence(nodePepRefined.Peptide.Sequence,
                        IsotopeLabelType.light, nodePepRefined.ExplicitMods);
                    // Skip peptides already added
                    if (includedPeptides.Contains(pepModSeq))
                    {
                        // Record repeated peptides for removing duplicate peptides later
                        if (repeatedPeptides != null)
                            repeatedPeptides.Add(pepModSeq);
                        continue;                        
                    }
                    // Record all peptides seen
                    includedPeptides.Add(pepModSeq);
                }

                listPeptides.Add(nodePepRefined);
            }

            return (PeptideGroupDocNode)nodePepGroup.ChangeChildrenChecked(listPeptides.ToArray(), true);
        }

// ReSharper disable SuggestBaseTypeForParameter
        private PeptideDocNode Refine(PeptideDocNode nodePep, SrmDocument document)
// ReSharper restore SuggestBaseTypeForParameter
        {
            int minTrans = MinTransitionsPepPrecursor ?? 0;

            bool addedGroups = false;
            var listGroups = new List<TransitionGroupDocNode>();
            foreach (TransitionGroupDocNode nodeGroup in nodePep.Children)
            {
                if (!AddLabelType && RefineLabelType != null && RefineLabelType.Value == nodeGroup.TransitionGroup.LabelType)
                    continue;

                double? peakFoundRatio = nodeGroup.AveragePeakCountRatio;
                if (!peakFoundRatio.HasValue)
                {
                    if (RemoveMissingResults)
                        continue;
                }
                else
                {
                    if (MinPeakFoundRatio.HasValue)
                    {
                        if (peakFoundRatio < MinPeakFoundRatio.Value)
                            continue;
                    }
                    if (MaxPeakFoundRatio.HasValue)
                    {
                        if (peakFoundRatio > MaxPeakFoundRatio.Value)
                            continue;
                    }
                }

                TransitionGroupDocNode nodeGroupRefined = Refine(nodeGroup);
                if (nodeGroupRefined.Children.Count < minTrans)
                    continue;

                if (peakFoundRatio.HasValue)
                {
                    if (DotProductThreshold.HasValue)
                    {
                        float? dotProduct = nodeGroupRefined.AverageLibraryDotProduct;
                        if (dotProduct.HasValue && dotProduct.Value < DotProductThreshold.Value)
                            continue;
                    }
                }

                // If this precursor node is going to be added, check to see if it
                // should be added with another matching isotope label type.
                if (IsLabelTypeRequired(nodePep, nodeGroup, listGroups))
                {
                    // CONSIDER: This is a lot like some code in PeptideDocNode.ChangeSettings
                    Debug.Assert(RefineLabelType != null);  // Keep ReSharper from warning
                    var tranGroup = new TransitionGroup(nodePep.Peptide, nodeGroup.TransitionGroup.PrecursorCharge,
                                                        RefineLabelType.Value);
                    var settings = document.Settings;
                    string sequence = nodePep.Peptide.Sequence;
                    var explicitMods = nodePep.ExplicitMods;
                    TransitionDocNode[] transitions = nodePep.GetMatchingTransitions(
                        tranGroup, settings, explicitMods);

                    var nodeGroupMatch = new TransitionGroupDocNode(tranGroup,
                        settings.GetPrecursorMass(tranGroup.LabelType, sequence, explicitMods),
                        settings.GetRelativeRT(tranGroup.LabelType, sequence, explicitMods),
                        transitions ?? new TransitionDocNode[0], transitions == null);

                    nodeGroupMatch = nodeGroupMatch.ChangeSettings(settings, explicitMods, SrmSettingsDiff.ALL);

                    // Make sure it is measurable before adding it
                    if (settings.TransitionSettings.Instrument.IsMeasurable(nodeGroupMatch.PrecursorMz))
                    {
                        listGroups.Add(nodeGroupMatch);
                        addedGroups = true;
                    }
                }

                listGroups.Add(nodeGroupRefined);
            }

            // If groups were added, make sure everything is in the right order.
            if (addedGroups)
                listGroups.Sort(Peptide.CompareGroups);

            return (PeptideDocNode) nodePep.ChangeChildrenChecked(listGroups.ToArray(), true);
        }

// ReSharper disable SuggestBaseTypeForParameter
        private bool IsLabelTypeRequired(PeptideDocNode nodePep, TransitionGroupDocNode nodeGroup,
            IEnumerable<TransitionGroupDocNode> listGroups)
// ReSharper restore SuggestBaseTypeForParameter
        {
            // If not adding a label type, or this precursor is already the label type being added,
            // then no further work is required
            if (!AddLabelType || !RefineLabelType.HasValue || RefineLabelType.Value == nodeGroup.TransitionGroup.LabelType)
                return false;

            // If either the peptide or the list of new groups already contains the
            // label type to be added, then do not add
            foreach (TransitionGroupDocNode nodeGroupChild in nodePep.Children)
            {
                if (nodeGroupChild.TransitionGroup.PrecursorCharge == nodeGroup.TransitionGroup.PrecursorCharge &&
                        nodeGroupChild.TransitionGroup.LabelType == RefineLabelType.Value)
                    return false;
            }
            foreach (TransitionGroupDocNode nodeGroupAdded in listGroups)
            {
                if (nodeGroupAdded.TransitionGroup.PrecursorCharge == nodeGroup.TransitionGroup.PrecursorCharge &&
                        nodeGroupAdded.TransitionGroup.LabelType == RefineLabelType.Value)
                    return false;
            }
            return true;
        }

// ReSharper disable SuggestBaseTypeForParameter
        private TransitionGroupDocNode Refine(TransitionGroupDocNode nodeGroup)
// ReSharper restore SuggestBaseTypeForParameter
        {
            var listTrans = new List<TransitionDocNode>();
            foreach (TransitionDocNode nodeTran in nodeGroup.Children)
            {
                double? peakFoundRatio = nodeTran.AveragePeakCountRatio;
                if (!peakFoundRatio.HasValue)
                {
                    if (RemoveMissingResults)
                        continue;
                }
                else
                {
                    if (MinPeakFoundRatio.HasValue)
                    {
                        if (peakFoundRatio < MinPeakFoundRatio.Value)
                            continue;
                    }
                    if (MaxPeakFoundRatio.HasValue)
                    {
                        if (peakFoundRatio > MaxPeakFoundRatio.Value)
                            continue;
                    }
                }

                listTrans.Add(nodeTran);
            }

            TransitionGroupDocNode nodeGroupRefined = (TransitionGroupDocNode)
                nodeGroup.ChangeChildrenChecked(listTrans.ToArray(), true);

            if (MaxPeakRank.HasValue)
            {
                // Calculate the average peak area for each transition
                int countTrans = nodeGroupRefined.Children.Count;
                var listAreaIndexes = new List<KeyValuePair<float, int>>();
                for (int i = 0; i < countTrans; i++)
                {
                    var nodeTran = (TransitionDocNode) nodeGroupRefined.Children[i];
                    listAreaIndexes.Add(new KeyValuePair<float, int>(nodeTran.AveragePeakArea ?? 0, i));                    
                }
                // Sort to area order descending
                listAreaIndexes.Sort((p1, p2) => Comparer.Default.Compare(p2.Key, p1.Key));
                // Store area ranks by transition index
                var ranks = new int[countTrans];
                for (int i = 0, iRank = 1; i < countTrans; i++)
                {
                    ranks[listAreaIndexes[i].Value] = iRank++;
                }

                // Add back all transitions with low enough rank.
                listTrans.Clear();
                for (int i = 0; i < countTrans; i++)
                {
                    if (ranks[i] > MaxPeakRank.Value)
                        continue;
                    listTrans.Add((TransitionDocNode) nodeGroupRefined.Children[i]);
                }

                nodeGroupRefined = (TransitionGroupDocNode)
                    nodeGroupRefined.ChangeChildrenChecked(listTrans.ToArray(), true);
            }

            return nodeGroupRefined;
        }
    }
}