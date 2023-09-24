using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using DyrdaDev.FirstPersonController;

public class InputActionBasedFirstPersonControllerInput : PlayerControllerInput
{
    #region Controller Input Fields

    public override IObservable<Vector2> Move => _move;
    private IObservable<Vector2> _move;

    public override IObservable<Unit> Stop => _stop;
    private Subject<Unit> _stop;

    public override IObservable<Unit> Jump => _jump;
    private Subject<Unit> _jump;

    public override ReadOnlyReactiveProperty<bool> Run => _run;
    private ReadOnlyReactiveProperty<bool> _run;

    public override IObservable<Vector2> Look => _look;
    private IObservable<Vector2> _look;

    public override IObservable<Unit> PlaceCanvas => _placeCanvas;
    private Subject<Unit> _placeCanvas;

    #endregion

    #region Configuration

    private FirstPersonInputAction _controls;

    #endregion

    private void OnEnable()
    {
        _controls.Enable();
    }

    private void OnDisable()
    {
        _controls.Disable();
    }

    protected void Awake()
    {
        _controls = new FirstPersonInputAction();

        // Hide the mouse cursor and lock it in the game window.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Move:
        _move = this.UpdateAsObservable()
            .Select(_ => _controls.Character.Move.ReadValue<Vector2>());

        //Stop:
        _stop = new Subject<Unit>().AddTo(this);
        _controls.Character.Move.canceled += context => _stop.OnNext(Unit.Default);

        // Jump:
        _jump = new Subject<Unit>().AddTo(this);
        _controls.Character.Jump.performed += context => _jump.OnNext(Unit.Default);

        // Run:
        _run = this.UpdateAsObservable()
            .Select(_ => _controls.Character.Run.ReadValueAsObject() != null)
            .ToReadOnlyReactiveProperty();

        // Look:
        _look = this.UpdateAsObservable()
            .Select(_ =>
            {
                return _controls.Character.Look.ReadValue<Vector2>();
            });

        //Place Canvas:
        _placeCanvas = new Subject<Unit>().AddTo(this);
        _controls.Character.PlaceCanvas.performed += context => _placeCanvas.OnNext(Unit.Default);
    }
}