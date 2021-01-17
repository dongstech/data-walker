[CmdletBinding()]
param()
begin
{
  Write-Output "Initiaing $applicationName ..."
  $applicationName = "Data-Walker"
  $requiredModules = @('ImportExcel')
  $workingDir = $PSScriptRoot
  $dataDir = "$workingDir\data"
  $inputDir = "$dataDir\input"
  $outputDir = "$dataDir\output"
  $resultFile = "$outputDir\result.xlsx"
  $metadataFile = "$dataDir\metadata.json"

  $sheetName = "For_Consol"
  $workingSpaceMarker = @{
    begin = "Begin"
    end   = "End_All"
  }

  $cellRangeMarker = 'T\d+'
  $emptyRows = 1
  $rangeRefColumnTitle = "From"

  $destStartCellCursor = @{}

  function Install-Dependence($requiredModules)
  {
    Write-Output "Checking dependences...."
    $requiredModules | ForEach-Object {
      $module = $_
      if ( -not (Get-Module -Name $module -ListAvailable))
      {
        Install-Module $module -Scope CurrentUser
      }
    }
  }

  function Get-ColumnNumber([string]$column)
  {
    if ($column.Length -gt 1)
    {
      throw "Illegal column name."
    }
    [int][char]$column - [int][char]'A' + 1
  }

  function Get-CellRange($worksheet, [OfficeOpenXml.ExcelPackage]$result, $fileName)
  {
    $tempArray = @()

    $workingspace = Get-WorkingSpace $worksheet
    $worksheet.Cells | ForEach-Object {
      $cell = $_
      if ( $cell.Start.Column -eq $workingspace.firstColumn -and $cell.Start.Row -gt $workingspace.firstRow -and $cell.Start.Row -lt $workingspace.lastRow)
      {
        $beginMarker = '^' + $cellRangeMarker
        if ($cell.Value -match $beginMarker)
        {
          $tempArray += $cell
        }
      }
      if ($cell.Start.Column -eq $workingspace.lastColumn -and $cell.Start.Row -gt $workingspace.firstRow -and $cell.Start.Row -lt $workingspace.lastRow)
      {
        $endMarker = '^End_' + $tempArray[$tempArray.Count - 1].Value
        if ($cell.Value -match $endMarker)
        {
          $tempArray += $cell
        }
      }
    }

    for ($i = 0; $i -lt $tempArray.Count; $i += 2)
    {
      $cell1 = $tempArray[$i]
      $cell2 = $tempArray[$i + 1]
      $sheetName = $cell1.Value
      Write-Host "$($cell1.Address) $($cell2.Address)"

      if ($result.Workbook.Worksheets | Where-Object { $_.Name -eq $cell1.Value } | Test-Any)
      {
        $destWorksheet = $result.Workbook.Worksheets[$sheetName]
      }
      else
      {
        $destWorksheet = $result.Workbook.Worksheets.Add($sheetName)
        $destWorksheet.SetValue(1,1, (Get-Date -Format "yyyy-MM-dd hh:mm:ss"))
      }
      $sourceRange = [OfficeOpenXml.ExcelRange]$worksheet.Cells[$cell1.Start.Row, $cell1.Start.Column, $cell2.Start.Row, $cell2.Start.Column]
      if ($destStartCellCursor.Keys -contains $sheetName)
      {
        $destStartCellCursor[$sheetName].row = $destStartCellCursor[$sheetName].row + $sourceRange.Rows + $emptyRows
      }
      else
      {
        $destStartCellCursor[$sheetName] = @{
          row    = 2
          column = 2
        }
      }
      $destStartCell = $destWorksheet.Cells[$destStartCellCursor[$sheetName].row, $destStartCellCursor[$sheetName].column]
      $sourceRange.Copy($destStartCell)
      $destWorksheet.SetValue($destStartCellCursor[$sheetName].row,1,$fileName)
      $destWorksheet.Cells.AutoFitColumns()
    }
  }

  function Get-CellRangeMarker($worksheet)
  {
    $markers = @()
    $workingspace = Get-WorkingSpace $worksheet
    $worksheet.Cells | ForEach-Object {
      $cell = $_
      if ( $cell.Start.Column -eq $workingspace.firstColumn -and $cell.Start.Row -gt $workingspace.firstRow -and $cell.Start.Row -lt $workingspace.lastRow)
      {
        if ($cell.Value.Length -gt 0)
        {
          $markers += @{
            begin = $cell.Value
            end   = "End_$($cell.Value)"
          }
        }
      }
    }
    $markers | ConvertTo-Json | Out-Host
    $markers
  }

  function Get-WorkingSpace($worksheet)
  {
    $workingspace = @{
      firstRow    = 0
      lastRow     = 0
      firstColumn = 0
      lastColumn  = 0
    }
    $worksheet.Cells | ForEach-Object {
      $cell = $_
      if ($workingSpaceMarker.begin -eq $cell.Value)
      {
        $workingspace.firstRow = $cell.Start.Row
        $workingspace.firstColumn = $cell.Start.Column
      }
      elseif ($workingSpaceMarker.end -eq $cell.Value)
      {
        $workingspace.lastRow = $cell.Start.Row + 1
        $workingspace.lastColumn = $cell.Start.Column
      }
    }
    $workingspace
  }

  function Test-Any()
  { 
    begin
    { 
      $any = $false
    } 
    process
    { 
      $any = $true
    } 
    end
    { 
      $any
    } 
  } 

  function Get-Summary($inputDir, $output)
  {
    $metadata1 = @()
    Get-ChildItem -Path $inputDir | ForEach-Object {
      Write-Host $_.BaseName
      $metadata1 += @{
        fileName = $_.BaseName
      }
    }
    ConvertTo-Json $metadata1 | Out-File -FilePath $output

  }

  Install-Dependence $requiredModules
  Write-Output "Initiate Success."
}

process
{
  Get-Summary $inputDir $metadataFile

  if (Test-Path -Path $resultFile)
  {
    Remove-Item -Path $resultFile
  }

  Export-Excel -Path $resultFile
  Get-ChildItem -Path $inputDir | ForEach-Object {
    $fileName = $_.BaseName
    try
    {
      $result = Open-ExcelPackage $resultFile
      $file = Open-ExcelPackage -Path $_.FullName
      $worksheet = $file.Workbook.Worksheets[$sheetName]
      Get-CellRange $worksheet $result $fileName
      # $delta = 0
      # $cellRanges.keys | ForEach-Object {
      #   $key = $_
      #   $cellRange = $cellRanges.$key
      #   Write-Host "============$key=============="
      #   Write-Host $cellRange.GetType()
      #   try
      #   {
      #     $result.Workbook.Worksheets.Add($key)
      #   }
      #   catch
      #   {
      #     Write-Debug "Sheet $key exists."
      #   }
      #   # $resultWorkSheet = $result.Workbook.Worksheets[$key]
      #   # $cellRange.Copy($resultWorkSheet.Cells)
      #   # $cellRange.Copy($resultWorkSheet.Cells[$cellRange.Start.Row, $cellRange.Start.Column, $cellRange.End.Row, $cellRange.End.Column])
      # }
    }
    finally
    {
      Close-ExcelPackage -ExcelPackage $file -NoSave
      Close-ExcelPackage -ExcelPackage $result
    }
  }
}
end
{

}