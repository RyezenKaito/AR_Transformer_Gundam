Add-Type -AssemblyName System.Drawing
$taskRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../../Assets/Models/SkyDefender/Textures'))
[System.IO.Directory]::CreateDirectory($taskRoot) | Out-Null
$taskImage = New-Object System.Drawing.Bitmap 1024,1024
$taskCanvas = [System.Drawing.Graphics]::FromImage($taskImage)
$taskCanvas.Clear([System.Drawing.Color]::Transparent)
$taskCanvas.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$taskCanvas.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
$taskDark = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(240,20,28,35))
$taskGold = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(245,207,169,100))
$taskFont = New-Object System.Drawing.Font 'Bahnschrift',22,([System.Drawing.FontStyle]::Bold)
$taskSmall = New-Object System.Drawing.Font 'Bahnschrift',13,([System.Drawing.FontStyle]::Regular)
$taskLarge = New-Object System.Drawing.Font 'Bahnschrift',64,([System.Drawing.FontStyle]::Bold)
$taskWords = @('GN-0000','SD / 02','CAUTION','NO STEP','01','02','03','04','SKY DEFENDER','DANGER','SD-02F','SD-02R','LIFT HERE','ACCESS','REACTOR','FLIGHT UNIT')
for($taskIndex=0;$taskIndex -lt 16;$taskIndex++) {
    $taskX = ($taskIndex % 4)*256
    $taskY = [math]::Floor($taskIndex/4)*256
    $taskBrush = if($taskIndex -ge 3 -and $taskIndex -le 7) { $taskGold } else { $taskDark }
    $taskPen = New-Object System.Drawing.Pen $taskBrush,3
    $taskCanvas.DrawLine($taskPen,$taskX+18,$taskY+43,$taskX+230,$taskY+43)
    if($taskIndex -ge 4 -and $taskIndex -le 7) {
        $taskCanvas.DrawString($taskWords[$taskIndex],$taskLarge,$taskBrush,[single]($taskX+45),[single]($taskY+54))
    } else {
        $taskCanvas.DrawString($taskWords[$taskIndex],$taskFont,$taskBrush,[single]($taskX+18),[single]($taskY+69))
        $taskCanvas.DrawString('CUSTOM SPECIFICATION',$taskSmall,$taskBrush,[single]($taskX+20),[single]($taskY+112))
        $taskCanvas.DrawString('G-CONCEPT  //  0000',$taskSmall,$taskBrush,[single]($taskX+20),[single]($taskY+140))
    }
    for($taskBar=0;$taskBar -lt 25;$taskBar++) { $taskCanvas.FillRectangle($taskBrush,[single]($taskX+22+$taskBar*7),[single]($taskY+186),[single](2+($taskBar%3)),[single]19) }
    $taskCanvas.DrawLine($taskPen,$taskX+18,$taskY+218,$taskX+230,$taskY+218)
    $taskPen.Dispose()
}
$taskImage.Save((Join-Path $taskRoot 'TechnicalStencils_Atlas.png'),[System.Drawing.Imaging.ImageFormat]::Png)
$taskCanvas.Dispose(); $taskImage.Dispose(); $taskDark.Dispose(); $taskGold.Dispose(); $taskFont.Dispose(); $taskSmall.Dispose(); $taskLarge.Dispose()
