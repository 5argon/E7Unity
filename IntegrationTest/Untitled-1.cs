public enum InputType 
{
    KeyboardMouse,
    Vive
}

public interface IControllable {}

public interface IKeyboardControllable: IControllable
{
    void A();
    void S();
    void D();
    void F();
}

public interface IXBoxControllerControllable  : IControllable
{
    void A();
    void B();
    void L();
    void R();
}

public class ControllerBehaviour
{
    protected InputType InputType { get;}
    public ControllerBehaviour(InputType inputType)
    {
        InputType = inputType;
    }
}

public class ShipBehaviour : ControllerBehaviour, IKeyboardControllable, IXBoxControllerControllable
{
    void IKeyboardControllable.A() => Fire();
    void IKeyboardControllable.S() => Fire();
    void IKeyboardControllable.D() => Boost();
    void IKeyboardControllable.F() => Boost();

    void IXBoxControllerControllable.A() => Fire();
    void IXBoxControllerControllable.B() => Fire();
    void IXBoxControllerControllable.L() => Fire();
    void IXBoxControllerControllable.R() => Boost();

    public ShipBehaviour(InputType inputType) : base(inputType) { }

    public void Fire(){}
    public void Boost(){}
}

