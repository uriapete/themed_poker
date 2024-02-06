using Godot;
using System;

public partial class LicensesScreen : CanvasLayer
{
    [Export]public RichTextLabel LicenseTextDisplay { get;private set; }

    public PackedScene MainMenuScene
    {
        get
        {
            return Global.CurrentInstance.MainMenuScene;
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        string licenseText = "";
        licenseText += $"This game is built with Godot Engine, available under the following license:\n\n{Engine.GetLicenseText()}";
        licenseText += $"\n\nThis game was also built with certain libraries, shown below:\n\n";
        licenseText += "Portions of this software are copyright © 1996-2022 The FreeType Project (www.freetype.org). All rights reserved.";
        licenseText += "\n\nThe ENet Library is included with this software, which is licensed under these terms:\n\nCopyright (c) 2002-2020 Lee Salzman\n\nPermission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the \"Software\"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:\n\nThe above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.\n\nTHE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.";
        licenseText += "\n\nThis software also includes mbed TLS:\n\nCopyright The Mbed TLS Contributors\n\nLicensed under the Apache License, Version 2.0 (the \"License\"); you may not use this file except in compliance with the License. You may obtain a copy of the License at\n\n[url]http://www.apache.org/licenses/LICENSE-2.0[/url]\n\nUnless required by applicable law or agreed to in writing, software distributed under the License is distributed on an \"AS IS\" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.";
        LicenseTextDisplay.Text = licenseText;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void BackToMainMenu()
    {
        GetTree().ChangeSceneToPacked(MainMenuScene);
    }
}
