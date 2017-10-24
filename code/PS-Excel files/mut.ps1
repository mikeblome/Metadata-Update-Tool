Param([string]$infile= "",[string]$outfile= "")


$excel = new-object -comobject excel.application
$excel.visible = $true



   $workbook = $excel.workbooks.open($infile)
   $Actions= "create,delete,IGNORE,OVERWRITE"
   $Tags = "title,ms.custom,ms.date,ms.reviewer,ms.suite,ms.technology,ms.tgt_pltform,mscorob.rating,ms.topic,dev_langs,ms.assetid,caps.latest.revision,author,manager,translation.priority.ht"
   $Formats= "single,dash"
   $worksheet = $workbook.worksheets.item(1)
   $worksheet.activate()
   $worksheet.Columns.item('A').ColumnWidth=60
   $worksheet.Columns.item('B').ColumnWidth=12
   $worksheet.Columns.item('C').ColumnWidth=20
   $worksheet.Columns.item('D').ColumnWidth=50
   $worksheet.Columns.item('E').ColumnWidth=12
   $worksheet.Columns.item('A:E').HorizontalAlignment.xlLeft
   $worksheet.Columns.item('A:E').wraptext=$true
   $worksheet.Columns.item('A:E').autofilter()

   $worksheet.Columns.item('B').Validation.add(3,1,1,$Actions)
   $worksheet.Columns.item('C').Validation.add(3,1,1,$Tags)
   $worksheet.Columns.item('E').Validation.add(3,1,1,$Formats)
   $headers = $worksheet.Range("a1:e1")
   $headers.Font.Bold=$true