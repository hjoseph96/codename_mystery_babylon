using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderSelection : Menu
{
    public Slider slider;
    public OptionsSlider OptionsSlider;
    [Tooltip("check yes if will change volume properties")]
    public bool volumeSlider;

    private void Start()
    {
        if (volumeSlider)
            _maxLoopSpeed = 0.001f;
    }

    public override void Activate()
    {
        
    }

    public override void Deactivate()
    {
        
    }

    public override void ProcessInput(InputData input)
    {
        HandleDirectionalMovement(input);

        switch (input.KeyCode)
        {
            case KeyCode.Z:
            case KeyCode.X:
                if (input.KeyState == KeyState.Up)
                {

                    slider.onValueChanged?.Invoke(slider.value);
                    UserInput.Instance.InputTarget = PreviousMenu;
                }
                break;
        }
    }

    protected override void HandleDirectionalMovement(InputData input)
    {
        if (input.MovementVector != Vector2Int.zero)
        {
            _currentTime += Time.deltaTime;
            if (_currentTime >= _currentLoopSpeed)
            {
                _currentTime -= _currentLoopSpeed;
                _loopedInput = true;

                if (_currentLoopSpeed > _maxLoopSpeed)
                {
                    _currentLoopSpeed -= _timeToIncreasePerLoop;
                }
            }
        }
        if (_zeroed || _loopedInput)
        {
            MoveSlider(-input.MovementVector.x);
            _zeroed = false;
            _loopedInput = false;
        }

        if (!_zeroed && input.MovementVector == Vector2Int.zero)
        {
            _zeroed = true;
            _currentLoopSpeed = _defaultLoopSpeed;
            _currentTime = 0f;
        }
    }

    private void MoveSlider(int input)
    {
        if (input == 0)
            return;

        float value;
        if (volumeSlider)
            value = Mathf.Clamp(slider.value - 0.01f * input, slider.minValue, slider.maxValue);
        else
            value = Mathf.Clamp(slider.value - input, slider.minValue, slider.maxValue);

        Debug.Log(value);
        slider.value = value;
        Debug.Log(slider.value);
    }
}
