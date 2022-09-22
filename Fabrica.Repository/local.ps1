Import-Module Fabrica.One/Builder

clear

dotnet publish E:\repository\fabrica\Fabrica.Repository -c:Release -p:PublishProfile=ForRelease

New-Appliance -Name fabrica-repository -Build local -Source E:\repository\fabrica\Fabrica.Repository\bin\Release\net6.0\publish -Region us-east-2 -Bucket pondhawk-appliance-repository -GenerateLatest $false


