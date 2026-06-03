
$path = 'f:\Projeler\OpenSourceHub\src\OpenSourceHub.Localization\LocalizationManager.cs'
$c = [System.IO.File]::ReadAllText($path, [System.Text.Encoding]::UTF8)
$idx = $c.LastIndexOf('}')
$c = $c.Substring(0, $idx).TrimEnd()
[System.IO.File]::WriteAllText($path, $c + "`n}", [System.Text.Encoding]::UTF8)
$lines = ($c -split "`n").Count
Write-Host "Fixed. Lines: $lines"
