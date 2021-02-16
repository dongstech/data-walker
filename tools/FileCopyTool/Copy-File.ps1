[CmdletBinding()]
param()
begin {
    $workingDir = $PSScriptRoot
    $outputDir = "$($workingDir)\output"
    $requiredModules = @('ImportExcel')
    function Install-Dependence($requiredModules) {
        Write-Output "Checking dependences...."
        $requiredModules | ForEach-Object {
            $module = $_
            if ( -not (Get-Module -Name $module -ListAvailable)) {
                Install-Module $module -Scope CurrentUser
            }
        }
    }
    function Load-Config() {
        Write-Output "Loading configuration..."
        $configFileName = "config.xlsx"
        Import-Excel "$workingDir/$configFileName"
    }
    
    function New-LogFile($logs) {
        Write-Host (ConvertTo-Json $logs)
        $logs | Select-Object -Property SourcePath,FileName,FileModified | Export-Excel -Path "$outputDir/log$DestFolderSufix.xlsx" -AutoSize
    }
    Write-Output "Initializing ..."
    Install-Dependence $requiredModules
    Write-Output "Initialize Success."
    $config = Load-Config
}
process {
    $logs = @()
    $destFolderSufix = Get-Date -Format "_yyyy-MM-dd_HHmm"
    $config | ForEach-Object {
        $sourcePath = $_.SourcePath
        $sourceRootFolder = $_.SourceRootFolder
        if (-not $sourcePath) {
            return
        }
        $sourceRootFolderFullPath = "$sourcePath/$sourceRootFolder"
        $sourceParentFolder = $_.SourceParentFolder
        $destFolder = $_.destinationFolder
        $destFolderFullName = "$outputDir/$destFolder$destFolderSufix/"
        if (-not (Test-Path $destFolderFullName)) {
            New-Item -Path $destFolderFullName -ItemType Directory
        }
        $folders = Get-ChildItem -Path $sourceRootFolderFullPath -Directory -Recurse | Where-Object -Property Name -eq $sourceParentFolder
        $folders | ForEach-Object {
            $folder = $_
            $files = Get-ChildItem -Path $folder -File 
            $files | ForEach-Object {
                $file = $_
                Write-Host "Copying $($file.FullName) to  ===> $destFolderFullName"
                Copy-Item -Path $file.FullName  -Destination $destFolderFullName
                $logs += @{
                    SourcePath   = Split-Path -Path $file.FullName
                    FileName     = $file.Name
                    FileModified = $file.LastWriteTime
                }
            }
        }
    }
    New-LogFile $logs
}
end {

}