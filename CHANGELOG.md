#ChangeLog
## [2.3.1] (05/02/2024)
- ### Changed
- - Updated dependency `com.cobilas.unity.utility` to version `2.10.3`.
- - This update includes bug fixes and new features that do not directly impact this package.
- - The sub-dependency `com.cobilas.unity.core.net4x@1.4.1` was made explicit in the package dependencies.
## [2.3.0] 27/01/2024
### Changed
A change in package dependencies.
## [2.2.0] - 06/09/2023
### Changed
- Package dependencies have been changed.
### Added
- The `com.unity.editorcoroutines` dependency has been added.
- The private static method `IEnumerator DebugConsole.WriteDebugLog(string, float);`
to write DebugLogger to the Editor.
## [2.1.0] - 29/08/2023
## Changed
- Package dependencies have been changed.
## [2.0.1-ch1] - 28/08/2023
### Changed
- The package author was changed from `Cobilas CTB` to `BélicusBr`.
## [2.0.1] - 26/04/2023
###Fixed
- The `Cobilas.Unity.Utility` namespace used in the `DebugConsole` class has been removed from the editor code.
### Changed
- The `Cobilas.Unity.UtilityConsole` namespace was replaced by `Cobilas.Unity.Utility.Console`.
## [1.0.2] - 30/01/2023
### Changed
- The method `string:DebugConsoleWin.ColoredText(DebugLogger)` was commented out as not being used.
- The fields of the `DebugLogger` class were marked as `readonly`.

## [1.0.0] 11/11/2022
### Repository com.cobilas.unity.debugconsole started
- Released to GitHub