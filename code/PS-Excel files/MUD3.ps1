#Param([string]$target= "")
$excel = new-object -comobject excel.application
$excel.visible = $true
# $excelFiles = Get-ChildItem -Path $target
#Foreach($file in $excelFiles)
#{
   $workbook = $excel.workbooks.open("C:\Users\stguty\Downloads\testfile.txt")
   $xlShiftDown = -4121
   $worksheet = $workbook.worksheets.item(1)
   
   $eRow = $worksheet.cells.item(1,1).entireRow
   $active = $eRow.activate()
   $active = $eRow.insert($xlShiftDown)
   $worksheet.Cells.Item(1,1) = "Asset/Path" 
   $worksheet.Cells.Item(1,2) = "Action" 
   $worksheet.Cells.Item(1,3) = "Tag" 
   $worksheet.Cells.Item(1,4) = "Value(s)" 
   $worksheet.Cells.Item(1,5) = "Format" 
   $worksheet.Columns.item('A').ColumnWidth=40
   $worksheet.Columns.item('B').ColumnWidth=12
   $worksheet.Columns.item('C').ColumnWidth=18
   $worksheet.Columns.item('D').ColumnWidth=40
   $worksheet.UsedRange.Columns.HorizontalAlignment.xlLeft
   $worksheet.Columns.item('A:D').wraptext=$true
   $worksheet.Columns.item('A:E').autofilter
   
   

   
   #$excel.Run("Format")
   #$workbook.save()
   #$workbook.close()
#}
#$excel.quit( )
