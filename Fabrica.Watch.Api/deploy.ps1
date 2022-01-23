param 
( 
	[Parameter(Mandatory=$false)] [String]$buildNumber, 
	[Parameter(Mandatory=$false)] [String]$contentDir,
	[Parameter(Mandatory=$false)] [String]$region,
	[Parameter(Mandatory=$false)] [String]$bucket
) 

Import-Module /usr/local/share/powershell/Modules/Fabrica.One/Builder

New-Appliance -Name fabrica-watch-api -Build $buildNumber -Source $contentDir -Region $region -Bucket $bucket
