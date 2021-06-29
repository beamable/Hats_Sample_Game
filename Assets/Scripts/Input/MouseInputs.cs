// GENERATED AUTOMATICALLY FROM 'Assets/Input/MouseInputs.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @MouseInputs : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @MouseInputs()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""MouseInputs"",
    ""maps"": [
        {
            ""name"": ""MouseInput"",
            ""id"": ""61c5dac8-6fd2-4462-9136-0c0c7959c23e"",
            ""actions"": [
                {
                    ""name"": ""Position"",
                    ""type"": ""Value"",
                    ""id"": ""05b716f7-9262-46e2-9558-068e415ad4c2"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Click"",
                    ""type"": ""Button"",
                    ""id"": ""988f2434-d32c-438f-9bc9-94c5073200f2"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""2bf0d9b2-b685-4fb3-b227-decd362b505e"",
                    ""path"": ""<Pointer>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Position"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""0d63477e-ddb5-4a19-b7f3-a02423e16289"",
                    ""path"": ""<Pointer>/press"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Click"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // MouseInput
        m_MouseInput = asset.FindActionMap("MouseInput", throwIfNotFound: true);
        m_MouseInput_Position = m_MouseInput.FindAction("Position", throwIfNotFound: true);
        m_MouseInput_Click = m_MouseInput.FindAction("Click", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // MouseInput
    private readonly InputActionMap m_MouseInput;
    private IMouseInputActions m_MouseInputActionsCallbackInterface;
    private readonly InputAction m_MouseInput_Position;
    private readonly InputAction m_MouseInput_Click;
    public struct MouseInputActions
    {
        private @MouseInputs m_Wrapper;
        public MouseInputActions(@MouseInputs wrapper) { m_Wrapper = wrapper; }
        public InputAction @Position => m_Wrapper.m_MouseInput_Position;
        public InputAction @Click => m_Wrapper.m_MouseInput_Click;
        public InputActionMap Get() { return m_Wrapper.m_MouseInput; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(MouseInputActions set) { return set.Get(); }
        public void SetCallbacks(IMouseInputActions instance)
        {
            if (m_Wrapper.m_MouseInputActionsCallbackInterface != null)
            {
                @Position.started -= m_Wrapper.m_MouseInputActionsCallbackInterface.OnPosition;
                @Position.performed -= m_Wrapper.m_MouseInputActionsCallbackInterface.OnPosition;
                @Position.canceled -= m_Wrapper.m_MouseInputActionsCallbackInterface.OnPosition;
                @Click.started -= m_Wrapper.m_MouseInputActionsCallbackInterface.OnClick;
                @Click.performed -= m_Wrapper.m_MouseInputActionsCallbackInterface.OnClick;
                @Click.canceled -= m_Wrapper.m_MouseInputActionsCallbackInterface.OnClick;
            }
            m_Wrapper.m_MouseInputActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Position.started += instance.OnPosition;
                @Position.performed += instance.OnPosition;
                @Position.canceled += instance.OnPosition;
                @Click.started += instance.OnClick;
                @Click.performed += instance.OnClick;
                @Click.canceled += instance.OnClick;
            }
        }
    }
    public MouseInputActions @MouseInput => new MouseInputActions(this);
    public interface IMouseInputActions
    {
        void OnPosition(InputAction.CallbackContext context);
        void OnClick(InputAction.CallbackContext context);
    }
}
