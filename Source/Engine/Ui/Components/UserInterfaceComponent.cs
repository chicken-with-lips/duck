using Duck.Content;
using Duck.Serialization;
using Duck.Ui.Assets;
using Duck.Ui.Scripting;

namespace Duck.Ui.Components;

[AutoSerializable]
public partial struct UserInterfaceComponent
{
    public string ContextName = String.Empty;
    public IAssetReference<UserInterface> ?Interface = default;
    public IUserInterfaceScript? Script = default;
}
