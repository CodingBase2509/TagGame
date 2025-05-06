using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TagGame.Shared.Domain.Players;

namespace TagGame.Client.Ui.Components;

public partial class PlayerListItem : ContentView
{
    public static BindableProperty PlayerProperty = BindableProperty.Create(
        nameof(Player), 
        typeof(Player), 
        typeof(PlayerListItem),
        propertyChanged: OnPlayerOrAdminChanged);
    
    public static BindableProperty AdminPlayerIdProperty = BindableProperty.Create(
        nameof(AdminPlayerId),
        typeof(Guid),
        typeof(PlayerListItem),
        propertyChanged: OnPlayerOrAdminChanged);
    
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

    public Guid AdminPlayerId
    {
        get => (Guid)GetValue(AdminPlayerIdProperty);
        set => SetValue(AdminPlayerIdProperty, value);
    }

    public string PlayerName => Player?.UserName ?? string.Empty;
    public string PlayerType => Player?.Type.GetDescription() ?? string.Empty;
    public bool IsPlayerAdmin => Equals(Player?.Id, AdminPlayerId);
    public Color IconColor => Player?.AvatarColor.ToMauiColor() ?? Colors.Black;
    
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
        OnPropertyChanged(nameof(PlayerType));
        OnPropertyChanged(nameof(IsPlayerAdmin));
        OnPropertyChanged(nameof(IconColor));
    }
}