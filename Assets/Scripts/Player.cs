    using System.Collections;
    using System.Collections.Generic;
using System.ComponentModel;
using Fusion;
using TMPro;
using Unity.VisualScripting;
    using UnityEngine;
using UnityEngine.Rendering;

public class Player : NetworkBehaviour
    {
        private NetworkCharacterController _cc;
        [SerializeField] private Ball _prefabBall;
        [SerializeField] private PhysxBall _prefabPhysxBall;
        private Vector3 _forward = Vector3.forward;
        
        [Networked] private TickTimer delay { get; set; }

        [Networked] public bool spawnedProjectile { get; set; }

    private ChangeDetector _changeDetector;

    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }

    private Material _material;


    private void Awake()
    {
        _cc = GetComponent<NetworkCharacterController>();
        _material = GetComponentInChildren<MeshRenderer>().material;
    }

    public override void Render()
    {
        foreach (var change in _changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(spawnedProjectile):
                    _material.color = Color.white;
                    break;
            }
        }
        _material.color = Color.Lerp(_material.color, Color.blue, Time.deltaTime);
    }





    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            data.direction.Normalize();
            _cc.Move(5 * data.direction * Runner.DeltaTime);
        }

        if (HasStateAuthority && delay.ExpiredOrNotRunning(Runner))
        {
            if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON0))
            {
                if (data.direction.sqrMagnitude > 0)
                    _forward = data.direction;

                if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON0))
                {
                    Debug.Log(data.buttons.IsSet(NetworkInputData.MOUSEBUTTON0));
                    Debug.Log(data.buttons.IsSet(NetworkInputData.MOUSEBUTTON0));
                    delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                    Runner.Spawn(_prefabBall,
                    transform.position + _forward, Quaternion.LookRotation(_forward),
                    Object.InputAuthority,
                    (runner, o) =>
                    {
                        o.GetComponent<Ball>().init();
                    });
                    spawnedProjectile = !spawnedProjectile;

                }



            }
            else if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON1))
            {
                if (data.direction.sqrMagnitude > 0)
                    _forward = data.direction;
                // Debug.Log(data.buttons.IsSet(NetworkInputData.MOUSEBUTTON1));
                delay = TickTimer.CreateFromSeconds(Runner, 0.12f);
                Runner.Spawn(_prefabPhysxBall,
                transform.position + _forward,
                Quaternion.LookRotation(_forward),
                Object.InputAuthority,
                (runner, o) =>
                {
                    o.GetComponent<PhysxBall>().Init(10 * _forward);
                });
                spawnedProjectile = !spawnedProjectile;
            }
        }


    }

    private void Update()
    {
        if (Object.HasStateAuthority && Input.GetKeyDown(KeyCode.R))
        {
            RPC_SendMessage("Hey Mate!");
        }
    }
        private TMP_Text _message;
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_SendMessage(string message, RpcInfo info = default)
    {
        RPC_RelayMessage(message, info.Source);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_RelayMessage(string message, PlayerRef messageSource)
    {
        if (_message == null)
            _message = FindObjectOfType<TMP_Text>();

        if (messageSource == Runner.LocalPlayer)
        {
            message = $"You said: {message}\n";
        }
        else
        {
            message = $"Some other player said: {message}\n";
        }
        _message.text += message;
    }

    }
