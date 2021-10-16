using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;

public class FakeMenuCommandService : IMenuCommandService
{
    readonly List<MenuCommand> addedCommands = new List<MenuCommand>();

    public IReadOnlyCollection<MenuCommand> AddedCommands
    {
        get { return new ReadOnlyCollection<MenuCommand>(addedCommands); }
    }

    public void AddCommand(MenuCommand command)
    {
        addedCommands.Add(command);
    }

    public void ExecuteCommand(int commandId)
    {
        var command = addedCommands.Find(_ => _.CommandID.ID == commandId);
        if (command != null)
        {
            command.Invoke();
        }
    }

    public void AddVerb(DesignerVerb verb)
    {
        throw new NotImplementedException();
    }

    public MenuCommand FindCommand(CommandID commandID)
    {
        throw new NotImplementedException();
    }

    public bool GlobalInvoke(CommandID commandID)
    {
        throw new NotImplementedException();
    }

    public void RemoveCommand(MenuCommand command)
    {
        throw new NotImplementedException();
    }

    public void RemoveVerb(DesignerVerb verb)
    {
        throw new NotImplementedException();
    }

    public void ShowContextMenu(CommandID menuID, int x, int y)
    {
        throw new NotImplementedException();
    }

    public DesignerVerbCollection Verbs { get; private set; }
}
