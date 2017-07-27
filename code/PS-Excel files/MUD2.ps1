Param([string]$target= "")
$target

$excel = new-object -comobject excel.application
$excel.visible = $true
# $excelFiles = Get-ChildItem -Path $target
#Foreach($file in $excelFiles)
#{
   $workbook = $excel.workbooks.open($target)
   
   $worksheet = $workbook.worksheets.item(1)
   $worksheet.Columns.item('A').ColumnWidth=40
   $worksheet.Columns.item('B').ColumnWidth=12
   $worksheet.Columns.item('C').ColumnWidth=18
   $worksheet.Columns.item('D').ColumnWidth=40
   $worksheet.UsedRange.Columns.HorizontalAlignment.xlLeft

   $worksheet.Columns.item('A:D').wraptext=$true

   
   #$excel.Run("Format")
   #$workbook.save()
   #$workbook.close()
#}
#$excel.quit( )
