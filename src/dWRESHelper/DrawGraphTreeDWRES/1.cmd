set src=F:\Developer\CURRENT\TMPApps\dWRESHelper\DrawGraphTreeDWRES\..\Libs\
set tgt=F:\Developer\CURRENT\TMPApps\dWRESHelper\DrawGraphTreeDWRES\..\..\out\Release\DWRESGraphBuilder\
robocopy /R:0 /S %src% %tgt%
set rce=%errorlevel%
echo %rce%
pause