using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Sand : NetworkBehaviour
{
    [SerializeField]
    SpriteRenderer m_SandSprite;

    [SerializeField]
    Collider2D m_Collider;

    [SerializeField]
    GameObject m_ChestImage;

    // Start is called before the first frame update
    void Start()
    {
        m_ChestImage.SetActive(false);
        m_Collider.enabled = true;
    }

    [Command(requiresAuthority = false)]

    public void ClientOpen()
    {
        DoOpen();
    }

    [ClientRpc]

    public void DoOpen()
    {
        m_Collider.enabled = false;

        Color _transparent = Color.white;
        _transparent.a = 0;

        m_SandSprite.color = _transparent;

        if (Gameplay.instance.WinningChest == this)
        {
            m_ChestImage.SetActive(true);
            string _text = "You lose!";

            if (isServer)
                Gameplay.instance.SetStatus(_text);
            else
                Gameplay.instance.DoSetStatus(_text);

            Gameplay.instance.StartCoroutine(Gameplay.instance.GameFinished());
        }
    }
}
