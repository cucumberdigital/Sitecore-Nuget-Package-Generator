param($installPath, $toolsPath, $package, $project)
 
write-host ===================================================
write-host "Setting 'CopyLocal' to false for the following references:"
 
$asms = $package.AssemblyReferences | %{$_.Name}
 
foreach ($reference in $project.Object.References)
{
    if ($asms -contains $reference.Name + ".dll")
    {
        Write-Host $reference.Name
        $reference.CopyLocal = $false;
    }
}