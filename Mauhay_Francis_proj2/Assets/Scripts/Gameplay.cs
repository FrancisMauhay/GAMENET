using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Gameplay : NetworkBehaviour
{
    public static Gameplay instance;
    public const string WIN_TEXT = "YOU WIN!";
    public const string GO_TEXT = "Go!";

    [SerializeField] GameObject m_SandObj;
    [SerializeField] Transform m_GridTransform;
    [SerializeField] TMPro.TextMeshProUGUI m_Status;

    private int m_RandGridX;
    private int m_RandGridY;

    [SyncVar] Player m_PlayerHost;
    [SyncVar] bool m_CanOpen;
    [SyncVar] Sand m_WinningChest;
    readonly SyncList<Sand> m_Sands = new SyncList<Sand>();

    public Sand WinningChest { get => m_WinningChest; }
    public bool CanOpen { get => m_CanOpen; set => m_CanOpen = value; }
    public Player PlayerHost { get => m_PlayerHost; set => m_PlayerHost = value; }

    void Awake()
    {
        instance = this;
        m_SandObj.SetActive(false);
        ForceSetText(GO_TEXT);
        m_CanOpen = true;  
    }

    void OnDestroy()
    {
        instance = null;
    }

    public void GenerateField(System.Action<GameObject> _toServerCallBack, System.Action<GameObject> _destroyObjCallback)
    {
        while (m_Sands.Count > 0)
        {
            _destroyObjCallback(m_Sands[0].gameObject);
            m_Sands.RemoveAt(0);
        }

        m_Sands.Clear();

        Queue<GameObject> _objects = new Queue<GameObject>();
        m_GridTransform.position = Vector2.zero;
        const float _spacing = 1.5f;
        m_RandGridX = Random.Range(3, 6);
        m_RandGridY = Random.Range(3, 6);
        Vector2Int _gridXY = new Vector2Int(m_RandGridX, m_RandGridY);
        Vector2 _gridTotalSize = new Vector2(_spacing*_gridXY.x - 1, _spacing*_gridXY.y - 1);

        for (int y = 0; y < _gridXY.x; y++)
        {
            for (int x = 0; x < _gridXY.y; x++)
            {
                GameObject _s = Instantiate(m_SandObj, m_GridTransform, true);
                _s.SetActive(true);
                _s.transform.localPosition = new Vector2(_spacing * x, _spacing * y);
                _objects.Enqueue(_s);
            }
        }

        int _winningIndex = Random.Range(0, _objects.Count);
        m_GridTransform.position = new Vector2(-_gridTotalSize.x/2, -_gridTotalSize.y/2);
        while (_objects.Count > 0)
        {
            GameObject _o = _objects.Dequeue();
            _o.transform.parent = null;
            _toServerCallBack(_o);
            m_Sands.Add(_o.GetComponent<Sand>());

            if (_winningIndex == 0)
                m_WinningChest = _o.GetComponent<Sand>();
            _winningIndex--;
        }
    }

    [Command(requiresAuthority = false)]
    public void DoSetStatus(string _text)
    {
        SetStatus(_text);
    }

    [ClientRpc]
    public void SetStatus(string _text)
    {
        if(m_Status.text != WIN_TEXT)
            m_Status.text = _text;
    }

    public void ForceSetText(string _text)
    {
        m_Status.text = _text;
    }

    public IEnumerator GameFinished()
    {
        m_CanOpen = false;
        yield return new WaitForSeconds(2F);
        m_PlayerHost.InitiateField();
        m_CanOpen = true;
        ForceSetText(GO_TEXT);
    }
}
