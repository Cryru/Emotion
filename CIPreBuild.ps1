(Get-Content .\EmotionCore\src\Meta.cs) -replace '000', $($env:APPVEYOR_BUILD_VERSION) | Set-Content .\EmotionCore\src\Meta.cs
(Get-Content .\EmotionCore\src\Meta.cs) -replace '><', $($env:APPVEYOR_REPO_COMMIT) | Set-Content .\EmotionCore\src\Meta.cs
(Get-Content .\EmotionCore\src\Meta.cs) -replace 'none', $($env:APPVEYOR_REPO_COMMIT_MESSAGE) | Set-Content .\EmotionCore\src\Meta.cs