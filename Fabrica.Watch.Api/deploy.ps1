param 
( 
	[Parameter(Mandatory=$false)] [String]$buildNumber, 
	[Parameter(Mandatory=$false)] [String]$contentDir,
	[Parameter(Mandatory=$false)] [String]$region,
	[Parameter(Mandatory=$false)] [String]$bucket
) 

Import-Module Fabrica.One/Builder

New-Appliance -Name fabrica-watch -Build $buildNumber -Source $contentDir -Region $region -Bucket $bucket
