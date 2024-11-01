using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] GameObject m_GameplayObj;
    Camera m_Cam;
    bool m_isInit;

    public override void OnStartServer()
    {
        base.OnStartServer();
        InitiateField();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        m_Cam = Camera.main;
    }

    [Command]
    public void InitiateField()
    {
        if (!isLocalPlayer)
            return;

        if(!m_isInit)
        {
            NetworkServer.Spawn(Instantiate(m_GameplayObj));
            Gameplay.instance.PlayerHost = this;
        }

        Gameplay.instance.GenerateField(
            (_obj) => { NetworkServer.Spawn(_obj); },
            (_obj) => { NetworkServer.Destroy(_obj); });

        if(!m_isInit)
            m_isInit = true;
    }

    void Update()
    {
        if(!isLocalPlayer)
            return;

        if(Input.GetMouseButtonDown(0) && Gameplay.instance.CanOpen)
        {
            RaycastHit2D _hit = Physics2D.Raycast(m_Cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if(_hit.collider != null)
            {
                if(_hit.collider.GetComponent<Sand>() != null)
                {
                    Sand _cell = _hit.collider.GetComponent<Sand>();
                    System.Action _ifWin = () =>
                    {
                        if (Gameplay.instance.WinningChest == _cell)
                            Gameplay.instance.ForceSetText(Gameplay.WIN_TEXT);
                    };

                    if(isServer)
                    {
                        _ifWin();
                        _cell.DoOpen();
                    }
                    else
                    {
                        _ifWin();_cell.ClientOpen();
                    }
                }
            }
        }
    }
}
