Add-Type -AssemblyName System.Drawing
$taskRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../../Assets/Models/BlueFlame/Textures'))
[System.IO.Directory]::CreateDirectory($taskRoot) | Out-Null
$taskImage = New-Object System.Drawing.Bitmap 2048,1024
$taskCanvas = [System.Drawing.Graphics]::FromImage($taskImage)
$taskCanvas.Clear([System.Drawing.Color]::Transparent)
$taskCanvas.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$taskCanvas.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
$taskInk = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(240,20,33,49))
$taskRed = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(240,162,44,41))
$taskFont = New-Object System.Drawing.Font 'Bahnschrift',48,([System.Drawing.FontStyle]::Bold)
$taskSmall = New-Object System.Drawing.Font 'Bahnschrift',22,([System.Drawing.FontStyle]::Regular)
$taskLarge = New-Object System.Drawing.Font 'Bahnschrift',144,([System.Drawing.FontStyle]::Bold)
$taskWords = @('CH - 01','BLUEFLAME','CAUTION','NO STEP','01','02','ACCESS','FLIGHT UNIT')
for($taskIndex=0;$taskIndex -lt 8;$taskIndex++) {
    $taskX = ($taskIndex % 4)*512
    $taskY = [math]::Floor($taskIndex/4)*512
    $taskBrush = if($taskIndex -eq 2 -or $taskIndex -eq 3) { $taskRed } else { $taskInk }
    $taskPen = New-Object System.Drawing.Pen $taskBrush,4
    $taskCanvas.DrawLine($taskPen,$taskX+36,$taskY+70,$taskX+464,$taskY+70)
    if($taskIndex -eq 4 -or $taskIndex -eq 5) {
        $taskCanvas.DrawString($taskWords[$taskIndex],$taskLarge,$taskBrush,[single]($taskX+100),[single]($taskY+94))
    } else {
        $taskCanvas.DrawString($taskWords[$taskIndex],$taskFont,$taskBrush,[single]($taskX+36),[single]($taskY+115))
        $taskCanvas.DrawString('BLUEFLAME / SERVICE UNIT',$taskSmall,$taskBrush,[single]($taskX+38),[single]($taskY+214))
        $taskCanvas.DrawString('LOCK BEFORE OPERATION',$taskSmall,$taskBrush,[single]($taskX+38),[single]($taskY+256))
    }
    for($taskBar=0;$taskBar -lt 36;$taskBar++) { $taskCanvas.FillRectangle($taskBrush,[single]($taskX+42+$taskBar*11),[single]($taskY+353),[single](2+($taskBar%4)),[single]34) }
    $taskCanvas.DrawLine($taskPen,$taskX+36,$taskY+430,$taskX+464,$taskY+430)
    $taskPen.Dispose()
}
$taskImage.Save((Join-Path $taskRoot 'BlueFlame_ServiceMarkings.png'),[System.Drawing.Imaging.ImageFormat]::Png)
$taskCanvas.Dispose(); $taskImage.Dispose(); $taskInk.Dispose(); $taskRed.Dispose(); $taskFont.Dispose(); $taskSmall.Dispose(); $taskLarge.Dispose()
