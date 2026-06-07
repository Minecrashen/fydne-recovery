using PlayerRoles;
using Qurre.API;
using Qurre.API.Controllers;
using UnityEngine;

namespace Loli.Addons.Chat;

internal class AuthorLite
{
    readonly Player pl;

    internal AuthorLite(Player pl)
        => this.pl = pl;


    bool _factionSetuped = false;
    Faction _faction;

    bool _teamSetuped = false;
    Team _team;

    bool _positionSetuped = false;
    Vector3 _position;

    bool _isAdminSetuped = false;
    bool _isAdmin;

    string _clan = null;

    string _userId = null;


    internal Faction Faction
    {
        get
        {
            if (!_factionSetuped)
            {
                _faction = pl.RoleInformation.Faction;
                _factionSetuped = true;
            }

            return _faction;
        }
    }

    internal Team Team
    {
        get
        {
            if (!_teamSetuped)
            {
                _team = pl.RoleInformation.Team;
                _teamSetuped = true;
            }

            return _team;
        }
    }

    internal Vector3 Position
    {
        get
        {
            if (!_positionSetuped)
            {
                _position = pl.MovementState.Position;
                _positionSetuped = true;
            }

            return _position;
        }
    }

    internal bool IsAdmin
    {
        get
        {
            if (!_isAdminSetuped)
            {
                _isAdmin = pl.ItsAdmin();
                _isAdminSetuped = true;
            }

            return _isAdmin;
        }
    }

    internal string Clan
    {
        get
        {
            _clan ??= pl.GetClan();

            return _clan;
        }
    }

    internal string UserId
    {
        get
        {
            _userId ??= pl.UserInformation.UserId;

            return _userId;
        }
    }
}