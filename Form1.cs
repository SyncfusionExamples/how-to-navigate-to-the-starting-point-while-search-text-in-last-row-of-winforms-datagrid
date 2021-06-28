using Syncfusion.Data;
using Syncfusion.WinForms.DataGrid;
using Syncfusion.WinForms.DataGrid.Enums;
using Syncfusion.WinForms.DataGrid.Interactivity;
using Syncfusion.WinForms.GridCommon.ScrollAxis;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;

namespace SfDataGridDemo
{
    public partial class Form1 : Form
    {
        ObservableCollection<OrderInfo> list = new ObservableCollection<OrderInfo>();
        public Form1()
        {
            InitializeComponent();

            list.Add(new OrderInfo(1, "Maria Anders", "Germany", "BOTTM", "Berlin", new DateTime(2020, 06, 12), true));
            list.Add(new OrderInfo(2, "Ana Trujilo", "Mexico", "ANATR", "Mexico D.F.", null, true));
            list.Add(new OrderInfo(1003, "Antonio Moreno", "Mexico", "ANTON", "Mexico D.F.", new DateTime(2020, 06, 12), true));
            list.Add(new OrderInfo(1004, "Thomas Hardy", "UK", "AROUT", "London", new DateTime(2020, 06, 12), true));
            list.Add(new OrderInfo(1005, "Christina Berglund", "Sweden", "BOTTM", "Lula", new DateTime(2020, 06, 12), true));
            list.Add(new OrderInfo(1006, "Hanna Moos", "Germany", "BLAUS", "Mannheim", new DateTime(2020, 06, 12), true));
            list.Add(new OrderInfo(1007, "Frederique Citeaux", "France", "BLONP", "Strasbourg", new DateTime(2020, 06, 12), true));
            list.Add(new OrderInfo(1008, "Martin Sommer", "Germany", "BOLID", "Madrid", null, true));
            list.Add(new OrderInfo(1009, "Laurence Lebihan", "France", "BONAP", "Marseille", null, true));
            list.Add(new OrderInfo(1010, "Elizabeth Lincoln", "Canada", "BOTTM", "Tsawassen", new DateTime(2020, 06, 12), true));
            
            this.sfDataGrid1.AutoGenerateColumns = true;
            this.sfDataGrid1.DataSource = list;
            sfDataGrid1.SearchController = new SearchControllerExt(sfDataGrid1);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.sfDataGrid1.SearchController.FindNext("Germany");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.sfDataGrid1.SearchController.FindPrevious("BOTTM");
        }
    }

    public class SearchControllerExt : SearchController
    {
        public SearchControllerExt(SfDataGrid grid) :
        base(grid)
        {

        }

        public override bool FindPrevious(string text)
        {
            bool isSearched = false;

            var isSourceDataGrid = (bool)DataGrid.GetType().GetProperty("IsSourceDataGrid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(DataGrid);

            if (isSourceDataGrid)
            {
                var parentGrid = this.DataGrid.GetTopLevelParentDataGrid();
                if (parentGrid == null)
                    return false;

                var processParentGridDetailsViewPreviousIndex = parentGrid.SearchController.GetType().GetMethod("ProcessParentGridDetailsViewPreviousIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                return (bool)processParentGridDetailsViewPreviousIndex.Invoke(parentGrid.SearchController, new object[] { this.DataGrid });
            }

            if (this.DataGrid.View == null)
                return false;

            if (this.Provider == null)
                Provider = this.DataGrid.View.GetPropertyAccessProvider();

            if (string.IsNullOrEmpty(text))
            {
                ClearSearch();
                return false;
            }

            SearchText = text;

            int lastColumnIndex = GetLastColumnIndex(true);
            var previousRowColumnIndex = CurrentRowColumnIndex;
            int columnIndex = CurrentRowColumnIndex.ColumnIndex < 0 ? lastColumnIndex : GetPreviousColumnIndex(CurrentRowColumnIndex.ColumnIndex);
            int rowIndex = CurrentRowColumnIndex.RowIndex != -1 ? CurrentRowColumnIndex.RowIndex : GetLastDataRowIndex();
            rowIndex = CurrentRowColumnIndex.ColumnIndex == GetFirstColumnIndex(true) && !IsInDetailsViewIndex(CurrentRowColumnIndex.RowIndex) ? GetPreviousDataRowIndex(rowIndex) : rowIndex;
            var currentIndex = this.CurrentRowColumnIndex.RowIndex;
            while (rowIndex >= 0)
            {
                if (!IsInDetailsViewIndex(CurrentRowColumnIndex.RowIndex) && SetPreviousRowColumnIndex(new RowColumnIndex(rowIndex, columnIndex)))
                {
                    isSearched = true;
                    var rowGenerator = (RowGenerator)this.DataGrid.TableControl.GetType().GetProperty("RowGenerator", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(this.DataGrid.TableControl);
                    var dataRow = rowGenerator.Items.FirstOrDefault(row => row.RowIndex == rowIndex);
                    VisibleLineInfo columnLineInfo = this.DataGrid.TableControl.ScrollColumns.GetVisibleLineAtLineIndex(CurrentRowColumnIndex.ColumnIndex);
                    VisibleLineInfo rowLineLineInfo = this.DataGrid.TableControl.ScrollRows.GetVisibleLineAtLineIndex(CurrentRowColumnIndex.RowIndex);

                    if ((columnLineInfo == null || columnLineInfo.IsClipped) || (rowLineLineInfo == null || rowLineLineInfo.IsClipped))
                    {
                        var scrollInView = DataGrid.CurrentCell.GetType().GetMethod("ScrollInView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        scrollInView.Invoke(DataGrid.CurrentCell, new object[] { CurrentRowColumnIndex });
                    }
                    break;
                }

                rowIndex = GetPreviousDataRowIndex(rowIndex);
                columnIndex = lastColumnIndex;
            }

            if (rowIndex >= 0 && this.AllowHighlightSearchText)
            {
                //Invalidate the old row column index to update the current match.
                InvalidateRowColumnIndex(previousRowColumnIndex);

                //Invalidate the new row column index to update the current match.
                InvalidateRowColumnIndex(CurrentRowColumnIndex);
            }

            return isSearched;
        }


        public override bool FindNext(string text)
        {
            if (SearchText != text)
            {
                DataGrid.SearchController.GetType().GetProperty("CurrentRowColumnIndex", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).SetValue(DataGrid.SearchController, new RowColumnIndex(-1, -1));
            }

            bool isSearched = false;

            var isSourceDataGrid = (bool)DataGrid.GetType().GetProperty("IsSourceDataGrid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(DataGrid);

            if (isSourceDataGrid)
            {
                var parentGrid = this.DataGrid.GetTopLevelParentDataGrid();
                if (parentGrid == null)
                    return false;

                var processParentGridDetailsViewPreviousIndex = parentGrid.SearchController.GetType().GetMethod("ProcessParentGridDetailsViewNextIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                return (bool)processParentGridDetailsViewPreviousIndex.Invoke(parentGrid.SearchController, new object[] { this.DataGrid });
            }

            if (this.DataGrid.View == null)
                return false;

            if (this.Provider == null)
                Provider = this.DataGrid.View.GetPropertyAccessProvider();

            if (string.IsNullOrEmpty(text))
            {
                ClearSearch();
                return false;
            }

            SearchText = text;


            int firstColumnIndex = GetFirstColumnIndex(true);
            var previousRowColumnIndex = CurrentRowColumnIndex;

            int columnIndex = CurrentRowColumnIndex.ColumnIndex < 0 ? firstColumnIndex : this.GetNextColumnIndex(CurrentRowColumnIndex.ColumnIndex);

            int rowIndex = CurrentRowColumnIndex.RowIndex != -1 ? CurrentRowColumnIndex.RowIndex : GetFirstDataRowIndex();

            rowIndex = CurrentRowColumnIndex.ColumnIndex == GetLastColumnIndex(true) && !IsInDetailsViewIndex(rowIndex) ? this.GetNextDataRowIndex(rowIndex) : rowIndex;
           
            while (rowIndex >= 0)
            {
                if (!IsInDetailsViewIndex(rowIndex) && SetNextRowColumnIndex(new RowColumnIndex(rowIndex, columnIndex)))
                {
                    isSearched = true;
                    VisibleLineInfo columnLineInfo = this.DataGrid.TableControl.ScrollColumns.GetVisibleLineAtLineIndex(CurrentRowColumnIndex.ColumnIndex);
                    VisibleLineInfo rowLineLineInfo = this.DataGrid.TableControl.ScrollRows.GetVisibleLineAtLineIndex(CurrentRowColumnIndex.RowIndex);

                    if ((columnLineInfo == null || columnLineInfo.IsClipped) || (rowLineLineInfo == null || rowLineLineInfo.IsClipped))
                    {
                        var scrollInView = DataGrid.CurrentCell.GetType().GetMethod("ScrollInView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        scrollInView.Invoke(DataGrid.CurrentCell, new object[] { CurrentRowColumnIndex });
                    }

                    break;
                }

                rowIndex = GetNextDataRowIndex(rowIndex);
                columnIndex = firstColumnIndex;
            }

            if (rowIndex >= 0 && this.AllowHighlightSearchText)
            {
                //Invalidate the old row column index to update the previous match.
                InvalidateRowColumnIndex(previousRowColumnIndex);

                //Invalidate the new row column index to update the current match.
                InvalidateRowColumnIndex(CurrentRowColumnIndex);
            }

            return isSearched;
        }

        internal int GetPreviousColumnIndex(int columnIndex)
        {
            var resolvedIndex = this.DataGrid.TableControl.ResolveToGridVisibleColumnIndex(columnIndex);
            if (resolvedIndex >= this.DataGrid.Columns.Count)
                return GetLastColumnIndex(true);

            var gridColumn = this.DataGrid.Columns[resolvedIndex];

            if (gridColumn != null && gridColumn.ActualWidth != 0.0 && gridColumn.Visible)
            {
                gridColumn = this.GetNextColumn(columnIndex - 1, false);
                if (gridColumn != null)
                    return this.DataGrid.TableControl.ResolveToScrollColumnIndex(this.DataGrid.Columns.IndexOf(gridColumn));
                else
                    return GetLastColumnIndex(true);
            }

            return resolvedIndex;
        }


        internal int GetNextColumnIndex(int columnIndex)
        {
            var resolvedIndex = this.DataGrid.TableControl.ResolveToGridVisibleColumnIndex(columnIndex);
            if (resolvedIndex >= this.DataGrid.Columns.Count)
                return GetFirstColumnIndex(true);

            var gridColumn = this.DataGrid.Columns[resolvedIndex];

            if (gridColumn != null && gridColumn.ActualWidth != 0 && gridColumn.Visible)
            {
                gridColumn = this.GetNextColumn(columnIndex + 1, true);
                if (gridColumn != null)
                    return this.DataGrid.TableControl.ResolveToScrollColumnIndex(this.DataGrid.Columns.IndexOf(gridColumn));
                else
                    return GetFirstColumnIndex(true);

            }

            return resolvedIndex;
        }

        internal int GetNextDataRowIndex(int rowIndex)
        {
            if (rowIndex > GetLastDataRowIndex())
                return GetFirstDataRowIndex();
            if (rowIndex < GetFirstDataRowIndex())
                return GetFirstDataRowIndex();
            rowIndex = this.DataGrid.TableControl.ScrollRows.GetNextScrollLineIndex(rowIndex);
            NodeEntry nodeEntry = GetNodeEntry(rowIndex);
            if ((nodeEntry != null && nodeEntry.IsRecords) || nodeEntry is NestedRecordEntry)
                return rowIndex;
            else if (rowIndex >= 0)
                return GetNextDataRowIndex(rowIndex);
            return rowIndex;
        }

        internal int GetPreviousDataRowIndex(int rowIndex)
        {
            if (rowIndex < GetFirstDataRowIndex())
                return GetLastDataRowIndex();
            if (rowIndex > GetLastDataRowIndex())
                return GetLastDataRowIndex();

            rowIndex = this.DataGrid.TableControl.ScrollRows.GetPreviousScrollLineIndex(rowIndex);
            NodeEntry nodeEntry = GetNodeEntry(rowIndex);
            if ((nodeEntry != null && nodeEntry.IsRecords) || nodeEntry is NestedRecordEntry)
                return rowIndex;
            else
                return GetPreviousDataRowIndex(rowIndex);
        }

        private NodeEntry GetNodeEntry(int rowIndex)
        {
            if (rowIndex == -1 || DataGrid.IsAddNewRowIndex(rowIndex))
                return null;

            var gridModel = DataGrid.GetType().GetProperty("GridModel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(DataGrid) as GridModel;
            var hasGroup = (bool)gridModel.GetType().GetProperty("HasGroup", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(gridModel);

            if (hasGroup)
                return DataGrid.View.TopLevelGroup.DisplayElements[DataGrid.TableControl.ResolveToRecordIndex(rowIndex)];
            else
            {
                int recordIndex = DataGrid.TableControl.ResolveToRecordIndex(rowIndex);
                if (recordIndex < 0)
                    return null;
                RecordEntry nodeEntry = DataGrid.View.GetRecordAt(recordIndex) as RecordEntry;
                return nodeEntry;
            }
        }

        private bool SetPreviousRowColumnIndex(RowColumnIndex rowColumnIndex)
        {
            var record = this.DataGrid.GetRecordAtRowIndex(rowColumnIndex.RowIndex);
            int colIndex = this.DataGrid.TableControl.ResolveToGridVisibleColumnIndex(rowColumnIndex.ColumnIndex);
            for (int i = colIndex; i >= 0; i--)
            {
                var column = this.DataGrid.Columns[i];

                if (column != null && column.ActualWidth != 0.0 && column.Visible && (SearchColumns.Count == 0 || SearchColumns.Contains(column.MappingName)))
                {
                    if (MatchSearchText(column, record))
                    {
                        rowColumnIndex.ColumnIndex = DataGrid.TableControl.ResolveToScrollColumnIndex(i);
                        DataGrid.SearchController.GetType().GetProperty("CurrentRowColumnIndex", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).SetValue(DataGrid.SearchController, new RowColumnIndex(rowColumnIndex.RowIndex, rowColumnIndex.ColumnIndex));

                        if (DataGrid is DetailsViewDataGrid)
                        {
                            var detailsViewScrollInView = DataGrid.CurrentCell.GetType().GetMethod("DetailsViewScrollInView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            detailsViewScrollInView.Invoke(DataGrid.CurrentCell, new object[] { rowColumnIndex });
                        }
                        else
                            this.HorizontalScrollinView(rowColumnIndex.ColumnIndex);
                        return true;
                    }
                }
            }

            return false;
        }

        private bool SetNextRowColumnIndex(RowColumnIndex rowColumnIndex)
        {
            var record = this.DataGrid.GetRecordAtRowIndex(rowColumnIndex.RowIndex);
            int columnIndex = this.DataGrid.TableControl.ResolveToGridVisibleColumnIndex(rowColumnIndex.ColumnIndex);
            for (int i = columnIndex; i < this.DataGrid.Columns.Count; i++)
            {
                var column = this.DataGrid.Columns[i];

                if (column != null && column.ActualWidth != 0.0 && column.Visible && (SearchColumns.Count == 0 || SearchColumns.Contains(column.MappingName)))
                {
                    if (MatchSearchText(column, record))
                    {
                        rowColumnIndex.ColumnIndex = DataGrid.TableControl.ResolveToScrollColumnIndex(i);
                        DataGrid.SearchController.GetType().GetProperty("CurrentRowColumnIndex", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).SetValue(DataGrid.SearchController, new RowColumnIndex(rowColumnIndex.RowIndex, rowColumnIndex.ColumnIndex));

                        if (DataGrid is DetailsViewDataGrid)
                        {
                            var detailsViewScrollInView = DataGrid.CurrentCell.GetType().GetMethod("DetailsViewScrollInView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            detailsViewScrollInView.Invoke(DataGrid.CurrentCell, new object[] { rowColumnIndex });
                        }
                        else
                            this.HorizontalScrollinView(rowColumnIndex.ColumnIndex);
                        return true;
                    }
                }
            }

            return false;
        }

        private void InvalidateRowColumnIndex(RowColumnIndex rowColumnIndex)
        {
            if (rowColumnIndex.RowIndex > 0)
            {
                var cellRect = DataGrid.TableControl.GetCellRectangle(rowColumnIndex.RowIndex, rowColumnIndex.ColumnIndex, true);

                if (DataGrid is DetailsViewDataGrid)
                    DataGrid.GetTopLevelParentDataGrid().TableControl.Invalidate();
                else
                    DataGrid.TableControl.Invalidate(cellRect);
            }
        }

        internal int GetFirstColumnIndex(bool isSearching = false)
        {
            int firstIndex = DataGrid.Columns.IndexOf(DataGrid.Columns.FirstOrDefault(x => x.Visible && x.Width != 0d && (x.AllowFocus || isSearching)));
            if (firstIndex < 0)
                return firstIndex;
            firstIndex += DataGrid.View != null ? DataGrid.View.GroupDescriptions.Count : 0;

            var HasDetailsView = DataGrid != null && DataGrid.DetailsViewDefinitions != null && DataGrid.DetailsViewDefinitions.Any();

            if (HasDetailsView || DataGrid.ShowPreviewRow) firstIndex += 1;
            if (DataGrid.ShowRowHeader)
                firstIndex += 1;
            return firstIndex;
        }

        internal int GetFirstDataRowIndex()
        {
            var topBodyCount = DataGrid.GetUnboundRowsCount(VerticalPosition.Top, true);
            if (GetRecordsCount(false) == 0)
                return -1;

            var headerLineCount = (int)DataGrid.TableControl.GetType().GetProperty("HeaderLineCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(DataGrid.TableControl);

            int index = headerLineCount + topBodyCount;
            if (DataGrid.FilterRowPosition == RowPosition.Top)
                index += 1;

            if (DataGrid.AddNewRowPosition == RowPosition.Top)
                index += 1;

            int count = 0;
            for (int start = index; start >= 0; start--)
            {
                if (!DataGrid.TableControl.RowHeights.GetHidden(start, out count))
                    return start;
            }

            return index;
        }

        internal int GetRecordsCount(bool checkUnboundRows = true)
        {
            int index = 0;
            if (DataGrid.View != null)
                return DataGrid.View.Records.Count;

            if (checkUnboundRows)
            {
                var topBodyCount = DataGrid.GetUnboundRowsCount(VerticalPosition.Top, true);
                var bottomBodyCount = DataGrid.GetUnboundRowsCount(VerticalPosition.Bottom, false);
                index += topBodyCount + bottomBodyCount;
            }

            return index;
        }

        internal int GetLastColumnIndex(bool isSearching = false)
        {
            int lastIndex = DataGrid.Columns.IndexOf(DataGrid.Columns.LastOrDefault(x => x.Visible && (x.AllowFocus || isSearching) && x.Width != 0d));
            if (lastIndex < 0)
            {
                if (DataGrid.IsAddNewRowIndex(DataGrid.CurrentCell.RowIndex) || DataGrid.IsFilterRowIndex(DataGrid.CurrentCell.RowIndex))
                    return DataGrid.Columns.IndexOf(DataGrid.Columns.LastOrDefault(x => x.Visible && x.Width != 0d));
                else
                    return lastIndex;
            }

            lastIndex += DataGrid.View != null ? DataGrid.View.GroupDescriptions.Count : 0;

            var HasDetailsView = DataGrid != null && DataGrid.DetailsViewDefinitions != null && DataGrid.DetailsViewDefinitions.Any();

            if (HasDetailsView || DataGrid.ShowPreviewRow) lastIndex += 1;
            if (DataGrid.ShowRowHeader)
                lastIndex += 1;
            return lastIndex;
        }

        internal bool IsInDetailsViewIndex(int rowIndex)
        {
            var HasDetailsView = DataGrid != null && DataGrid.DetailsViewDefinitions != null && DataGrid.DetailsViewDefinitions.Any();

            if (!HasDetailsView || rowIndex < 0)
                return false;

            if (DataGrid.IsAddNewRowIndex(rowIndex) || DataGrid.IsFilterRowIndex(rowIndex) || DataGrid.IsUnboundRow(rowIndex) || DataGrid.TableControl.IsTableSummaryIndex(rowIndex))
                return false;

            var startIndex = DataGrid.TableControl.ResolveStartIndexBasedOnPosition();
            var counter = Math.Max(rowIndex - startIndex, 0);

            var gridModel = DataGrid.GetType().GetProperty("GridModel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(DataGrid) as GridModel;
            var hasGroup = (bool)gridModel.GetType().GetProperty("HasGroup", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(gridModel);

            if (hasGroup)
            {
                var displayElement = DataGrid.View.TopLevelGroup.DisplayElements[counter];
                return displayElement is NestedRecordEntry;
            }

            return (counter % (DataGrid.DetailsViewDefinitions.Count + 1)) != 0;
        }

        internal int GetLastDataRowIndex()
        {
            var footerCount = DataGrid.GetUnboundRowsCount(VerticalPosition.Bottom, true);
            var bottomBodyCount = DataGrid.GetUnboundRowsCount(VerticalPosition.Bottom, false);
            if (GetRecordsCount(false) == 0)
                return -1;
            int count = 0;
            int index = DataGrid.RowCount - (DataGrid.TableControl.GetTableSummaryCount(VerticalPosition.Bottom) + bottomBodyCount + footerCount + 1);
            if (DataGrid.AddNewRowPosition == RowPosition.Bottom || DataGrid.AddNewRowPosition == RowPosition.FixedBottom)
                index -= 1;
            if (DataGrid.FilterRowPosition == RowPosition.Bottom || DataGrid.FilterRowPosition == RowPosition.FixedBottom)
                index -= 1;

            for (int start = index; start >= 0; start--)
            {
                if (!DataGrid.TableControl.RowHeights.GetHidden(start, out count))
                    return start;
            }

            return index;
        }

    }

}


