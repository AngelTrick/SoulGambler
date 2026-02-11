using UnityEngine;

public class ExpGem : MonoBehaviour
{
    [Header("Setting")]
    public int expAmount = 10;

    private Transform _targetPlayer;
    private bool isMagnet = false;
    private float _magnetSpeed = 15f;

    private float _defaultMagnetRange = 2.0f;
    public void Initialize(Transform player)
    {
        _targetPlayer = player;
        isMagnet = false;
    }
    private void OnEnable()
    {
        if(PlayerController.Instance != null)
        {
            Initialize(PlayerController.Instance.transform);
        }
        isMagnet = false;
    }
    private void Update()
    {
        if(_targetPlayer == null) return;
        
        if (isMagnet)
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetPlayer.position, _magnetSpeed * Time.deltaTime);
        }
        else
        {
            float detectRange = _defaultMagnetRange;

            if(PlayerController.Instance != null )
            {
                detectRange = PlayerController.Instance.GetFinalMagnetRange();
            }
            if (Vector3.Distance(transform.position, _targetPlayer.position) < detectRange)
            {
                isMagnet = true;
            }
        } 
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if(GameManager.Instance != null) GameManager.Instance.GetExp(expAmount);
            gameObject.SetActive(false);
        }
    }
}
