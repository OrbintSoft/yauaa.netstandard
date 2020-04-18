$java = 'java.exe'
if (-Not (Get-Command $java -ErrorAction SilentlyContinue)) 
{ 
	echo "Java not found, probably enviroment path is not set"
	if (-Not (Test-Path env:JAVA_HOME) ) { 
		echo "JAVA_HOME is not set, it impossible to compile"
		exit -2
	} else {
		$java = "$($env:JAVA_HOME)\java.exe"
		if (-Not (Get-Command $java -ErrorAction SilentlyContinue)) 
		{
			echo "Java not found, it impossible to compile"
			exit -3
		}
	}
}
$antlr = ".\Dependencies\antlr-4.8-complete.jar"
if (-Not (Get-Command $antlr -ErrorAction SilentlyContinue)) 
{
	echo "antlr-4.8-complete.jar Not found, impossible to compile"
	exit -4
} else {
	java -jar ".\Dependencies\antlr-4.8-complete.jar" -Dlanguage=CSharp -visitor -package OrbintSoft.Yauaa.Antlr4Source -Xexact-output-dir -o ".\Antlr4Generated" ".\Antlr4Source\UserAgent.g4"
	java -jar ".\Dependencies\antlr-4.8-complete.jar" -Dlanguage=CSharp -visitor -package OrbintSoft.Yauaa.Antlr4Source -Xexact-output-dir -o ".\Antlr4Generated" ".\Antlr4Source\UserAgentTreeWalker.g4"
	exit 0;
}

