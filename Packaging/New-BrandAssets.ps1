$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$assetRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../Assets'))
function New-Mark([int]$width, [int]$height, [string]$path) {
    $bitmap = [Drawing.Bitmap]::new($width, $height)
    $graphics = [Drawing.Graphics]::FromImage($bitmap)
    try {
        $graphics.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::AntiAlias
        $graphics.Clear([Drawing.Color]::FromArgb(20, 58, 52))
        $size = [Math]::Min($width, $height) * 0.72
        $left = ($width - $size) / 2
        $top = ($height - $size) / 2
        $pen = [Drawing.Pen]::new([Drawing.Color]::FromArgb(220, 188, 126), [single]($size * 0.045))
        try {
            $graphics.DrawEllipse($pen, [single]$left, [single]$top, [single]$size, [single]$size)
            $graphics.DrawLine($pen, [single]($left + $size * .39), [single]($top + $size * .26), [single]($left + $size * .39), [single]($top + $size * .72))
            $graphics.DrawLine($pen, [single]($left + $size * .39), [single]($top + $size * .72), [single]($left + $size * .68), [single]($top + $size * .72))
            $points = [Drawing.PointF[]]@([Drawing.PointF]::new($left + $size * .64, $top + $size * .16), [Drawing.PointF]::new($left + $size * .54, $top + $size * .43), [Drawing.PointF]::new($left + $size * .76, $top + $size * .34))
            $brush = [Drawing.SolidBrush]::new([Drawing.Color]::FromArgb(220, 188, 126))
            try { $graphics.FillPolygon($brush, $points) } finally { $brush.Dispose() }
        } finally { $pen.Dispose() }
        $bitmap.Save($path, [Drawing.Imaging.ImageFormat]::Png)
    } finally { $graphics.Dispose(); $bitmap.Dispose() }
}
New-Mark 256 256 (Join-Path $assetRoot 'Lorevia.png')
$png = [IO.File]::ReadAllBytes((Join-Path $assetRoot 'Lorevia.png'))
$writer = [IO.BinaryWriter]::new([IO.File]::Create((Join-Path $assetRoot 'Lorevia.ico')))
try {
    $writer.Write([uint16]0); $writer.Write([uint16]1); $writer.Write([uint16]1)
    $writer.Write([byte]0); $writer.Write([byte]0); $writer.Write([byte]0); $writer.Write([byte]0)
    $writer.Write([uint16]1); $writer.Write([uint16]32); $writer.Write([uint32]$png.Length); $writer.Write([uint32]22); $writer.Write($png)
} finally { $writer.Dispose() }
New-Mark 88 88 (Join-Path $assetRoot 'Square44x44Logo.scale-200.png')
New-Mark 24 24 (Join-Path $assetRoot 'Square44x44Logo.targetsize-24_altform-unplated.png')
New-Mark 300 300 (Join-Path $assetRoot 'Square150x150Logo.scale-200.png')
New-Mark 620 300 (Join-Path $assetRoot 'Wide310x150Logo.scale-200.png')
New-Mark 100 100 (Join-Path $assetRoot 'StoreLogo.png')
