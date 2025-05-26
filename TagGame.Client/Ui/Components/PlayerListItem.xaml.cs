using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using TagGame.Client.Services;
using TagGame.Shared.Domain.Players;

namespace TagGame.Client.Ui.Components;

public partial class PlayerListItem : ContentView
{
    public static BindableProperty PlayerProperty = BindableProperty.Create(
        nameof(Player), 
        typeof(Player), 
        typeof(PlayerListItem),
        propertyChanged: OnPlayerOrAdminChanged);
    
    public static BindableProperty AdminUserIdProperty = BindableProperty.Create(
        nameof(AdminUserId),
        typeof(Guid),
        typeof(PlayerListItem),
        propertyChanged: OnPlayerOrAdminChanged);
    
    public static BindableProperty PlayerTypeChangedCommandProperty = BindableProperty.Create(
        nameof(PlayerTypeChangedCommand), 
        typeof(ICommand), 
        typeof(PlayerListItem));

    public static BindableProperty CanEditProperty = BindableProperty.Create(
        nameof(CanEdit),
        typeof(bool),
        typeof(PlayerListItem));
    
    public PlayerListItem()
    {
        InitializeComponent();
    }

    public Player Player
    {
        get => (Player)GetValue(PlayerProperty);
        set
        {
            SetValue(PlayerProperty, value);
        }
    }

    public Guid AdminUserId
    {
        get => (Guid)GetValue(AdminUserIdProperty);
        set => SetValue(AdminUserIdProperty, value);
    }
    
    public ICommand PlayerTypeChangedCommand
    {
        get => (ICommand)GetValue(PlayerTypeChangedCommandProperty);
        set => SetValue(PlayerTypeChangedCommandProperty, value);
    }

    public bool CanEdit
    {
        get => (bool)GetValue(CanEditProperty);
        set
        {
            OpenPlayerTypeSelectCommand.CanExecute(value);
            SetValue(CanEditProperty, value);
        }
    }
    
    public string PlayerName => Player?.UserName ?? string.Empty;
    public string Playertype => Player?.Type.GetDescription() ?? string.Empty;
    public bool IsPlayerAdmin => Equals(Player?.UserId, AdminUserId);
    public Color IconColor => Player?.AvatarColor.ToMauiColor() ?? Colors.Black;

    [RelayCommand]
    public async Task OpenPlayerTypeSelect()
    {
        if (Player is null || !CanEdit) 
            return;

        var loc = ServiceHelper.GetRequiredService<Localization>();
        string pageName = "LobbyPage";

        var values = Enum.GetValues<PlayerType>()
            .Select(pt => pt.GetDescription())
            .ToArray();
        
        var result = await Shell.Current.DisplayActionSheet(
            loc.Get("select-playertype", pageName),
            loc.Get("abort", pageName),
            "",
            FlowDirection.LeftToRight,
            values);

        if (!Enum.TryParse<PlayerType>(result, out var enumValue))
            return;
        
        if (enumValue is PlayerType newType && newType != Player.Type)
        {
            Player.Type = newType;
            NotifyAllDependentProperties();

            PlayerTypeChangedCommand?.Execute(Player);
        }
    }
    
    private static void OnPlayerOrAdminChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PlayerListItem control)
        {
            control.NotifyAllDependentProperties();
        }
    }

    private void NotifyAllDependentProperties()
    {
        OnPropertyChanged(nameof(PlayerName));
        OnPropertyChanged(nameof(Playertype));
        OnPropertyChanged(nameof(IsPlayerAdmin));
        OnPropertyChanged(nameof(IconColor));
    }
}