using System;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PhotonView))]
public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    #region Singleton

    public static PlayerController Instance;
    public static GameObject LocalPlayerInstance;

    #endregion

    #region Serialized Fields

    [Header("Movimentação")]
    [SerializeField] private float _playerSpeed = 10f;
    [SerializeField] private float _jumpForce = 10f;
    public Transform _meshTransform;

    [Header("Pulo")]
    public float groundCheckDistance = 1.1f;
    public LayerMask groundLayer;
    public float gravityMultiplier = 2f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    [Header("Ataque")]
    public float attackCooldown = 2f;
    public float attackRange = 1.5f;
    public LayerMask enemyLayer;

    [Header("Vida")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float respawnDelay = 3f;
    public Slider healthBarUI;

    #endregion

    #region Private Fields

    private int _currentHealth;
    private bool isDead = false;
    private Rigidbody _rb;
    private PhotonView _photonView;
    private TMP_Text _namePlayer;
    private Animator _animator;

    private string _nickname;
    private int _localScore;
    private float lastAttackTime;
    private bool _isGrounded;
    private Transform _localStartPosition;

    // Network sync
    private Vector3 _networkPosition;
    private Quaternion _networkRotation;
    private float _rotationLerpSpeed = 10f;

    #endregion

    #region Properties

    public Vector3 Movement { get; set; }
    public float JumpForce => _jumpForce;
    public float PlayerSpeed
    {
        get => _playerSpeed;
        set => _playerSpeed = value;
    }
    public bool PodeMover { get; private set; }
    public int TeamID { get; private set; }

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _photonView = GetComponent<PhotonView>();
        _animator = GetComponent<Animator>();
        _namePlayer = GetComponentInChildren<TMP_Text>();
        if (healthBarUI == null)
            healthBarUI = GetComponentInChildren<Slider>();

        if (_photonView.IsMine)
        {
            LocalPlayerInstance = gameObject;

            _nickname = PhotonNetwork.LocalPlayer.NickName;
            _namePlayer.text = _nickname;
            HabilitaMovimentacao(true);
            _localStartPosition = transform;
            TeamID = PhotonNetwork.LocalPlayer.ActorNumber; // Um time por jogador

            _currentHealth = maxHealth;
            UpdateHealthUI();
        }
        else
        {
            _namePlayer.text = _nickname;

            // Desativa câmera e áudio dos outros jogadores
            var cam = GetComponentInChildren<Camera>();
            if (cam != null) cam.enabled = false;

            var listener = GetComponentInChildren<AudioListener>();
            if (listener != null) listener.enabled = false;
        }
    }

    private void Update()
    {
        if (!_photonView.IsMine)
        {
            // Interpolação de posição e rotação recebidas pela rede
            transform.position = Vector3.Lerp(transform.position, _networkPosition, Time.deltaTime * _playerSpeed);
            _meshTransform.rotation = Quaternion.Lerp(_meshTransform.rotation, _networkRotation, Time.deltaTime * _rotationLerpSpeed);
            return;
        }

        HandleMovement();
        HandleJump();
        HandleAttack();
    }

    private void FixedUpdate()
    {
        ApplyCustomGravity();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_meshTransform.position + _meshTransform.forward, attackRange);
    }

    #endregion

    #region Core Systems

    private void HandleMovement()
    {
        _rb.velocity = new Vector3(0f, 0f, 0f);

        if (!PodeMover) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 inputDir = new Vector3(h, 0, v);

        // Normaliza para evitar velocidade maior na diagonal
        if (inputDir.sqrMagnitude > 1)
            inputDir.Normalize();

        Vector3 move = inputDir * _playerSpeed;
        _rb.velocity = new Vector3(move.x, _rb.velocity.y, move.z);

        _animator.SetFloat("move", move.sqrMagnitude > 0.001f ? 1 : -1);

        if (inputDir.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(h, v) * Mathf.Rad2Deg;
            _meshTransform.rotation = Quaternion.Euler(0, angle, 0);
        }

        CheckGround();
    }

    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z); // Zera Y para pulo limpo
            _rb.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
            _animator.SetTrigger("jump");
        }
    }

    private void ApplyCustomGravity()
    {
        if (_rb.velocity.y < 0)
        {
            _rb.AddForce(Vector3.up * Physics.gravity.y * (fallMultiplier - 1), ForceMode.Acceleration);
        }
        else if (_rb.velocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            _rb.AddForce(Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1), ForceMode.Acceleration);
        }
    }

    private void HandleAttack()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && Time.time >= lastAttackTime + attackCooldown)
        {
            _animator.SetTrigger("attack");
            lastAttackTime = Time.time;
            HabilitaMovimentacao(false);
            StartCoroutine(EnableMovementAfterDelay(attackCooldown));

            //Collider[] hitEnemies = Physics.OverlapSphere(_meshTransform.position + _meshTransform.forward, attackRange, enemyLayer);
            //foreach (Collider enemy in hitEnemies)
            //{
            //    PlayerController target = enemy.GetComponent<PlayerController>();
            //    DealDamageToPlayer();
            //}
        }
    }

    private void CheckGround()
    {
        _isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
    }

    #endregion

    #region Networking

    private void ApplyKnockback(Vector3 sourcePosition, float force)
    {
        Vector3 knockbackDir = (transform.position - sourcePosition).normalized;
        knockbackDir.y = 0f; // keep player grounded

        _rb.velocity = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z); // reset Y before applying
        _rb.AddForce(knockbackDir * force, ForceMode.Impulse);
    }

    private IEnumerator EnableMovementAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HabilitaMovimentacao(true);
    }

    public void DealDamageToPlayer()
    {
        // Apenas o dono da view envia o dano
        if (_photonView.IsMine) return;

        photonView.RPC("TakeDamage", photonView.Owner, transform.position, 10f);
    }

    [PunRPC]
    public void TakeDamage(Vector3 sourcePosition, float knockbackForce)
    {
        if (!_photonView.IsMine || isDead) return;

        Debug.Log("ESTOY AQUI!");

        ApplyKnockback(sourcePosition, knockbackForce);
        
        _animator.SetTrigger("takeDamage");

        _currentHealth -= 25; // ou o valor de dano

        UpdateHealthUI();

        if (_currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        isDead = true;
        PodeMover = false;
        _animator.SetTrigger("die");

        // Desativa visual, colisores, etc.
        //GetComponent<Collider>().enabled = false;
        _rb.velocity = Vector3.zero;

        // Notifica o master para atualizar o score
        if (PhotonNetwork.IsMasterClient)
            GameManager.Instance.RegisterKill(TeamID);

        StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnDelay);

        // Reseta vida
        _currentHealth = maxHealth;
        UpdateHealthUI();
        isDead = false;

        // Move para novo ponto de respawn
        Transform spawn = _localStartPosition;
        transform.position = spawn.position;

        // Reativa
        GetComponent<Collider>().enabled = true;
        _animator.SetTrigger("jump");
        PodeMover = true;
    }

    private void UpdateHealthUI()
    {
        
        healthBarUI.value = (float)_currentHealth / maxHealth;
        
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Score"))
        {
            Debug.Log((int)PhotonNetwork.LocalPlayer.CustomProperties["Score"]);
        }
    }

    [PunRPC]
    public void UpdateScore(int quantidade)
    {
        int scoreAtual = 0;

        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Score"))
        {
            scoreAtual = (int)PhotonNetwork.LocalPlayer.CustomProperties["Score"];
        }

        scoreAtual += quantidade;

        var tabela = new ExitGames.Client.Photon.Hashtable
        {
            { "Score", scoreAtual }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(tabela);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(_meshTransform.rotation);
            stream.SendNext(_nickname);
            stream.SendNext(_currentHealth);
        }
        else
        {
            _networkPosition = (Vector3)stream.ReceiveNext();
            _networkRotation = (Quaternion)stream.ReceiveNext();
            _nickname = (string)stream.ReceiveNext();
            _currentHealth = (int)stream.ReceiveNext();

            UpdateHealthUI(); // Update health bar from received value
        }
    }

    #endregion

    #region Public Methods

    public void HabilitaMovimentacao(bool mover)
    {
        PodeMover = mover;
    }

    #endregion
}
