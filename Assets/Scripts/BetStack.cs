using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BetStack : MonoBehaviour
{
    [SerializeField] private VerticalLayoutGroup _verticalLayoutGroup;
    [SerializeField] private Image[] _chips;
    [SerializeField] private Sprite[] _chipSprites;
    
    [SerializeField] private int _activeChips = 1;
    

    private float _minSpacing = 200f;
    private float _maxSpacing = 68f;
    
    private int _oldActiveChips;

    private bool _isConvertedStack = true;
    
    Dictionary<int, Sprite> _chipSpriteDictionary;

    private void Awake()
    {
        _oldActiveChips = _activeChips;

        _chipSpriteDictionary = new Dictionary<int, Sprite>()
        {
            {5, _chipSprites[0]},
            {10, _chipSprites[1]},
            {50, _chipSprites[2]},
            {100, _chipSprites[3]},
            {500, _chipSprites[4]}
        };
    }

    // private void Update()
    // {
    //     if (_oldActiveChips != _activeChips)
    //     {
    //         _oldActiveChips = _activeChips;
    //
    //         for (int i = _chips.Length - 1; i > 0; i--)
    //         {
    //             _chips[i].gameObject.SetActive(i <  _activeChips);
    //         }
    //
    //         var chipsProgress = ((float)(_activeChips-2) / (float)_chips.Length);
    //         var spacing = (-1) * Mathf.Lerp(_minSpacing, _maxSpacing, chipsProgress);
    //         _verticalLayoutGroup.spacing = spacing;
    //     }
    // }

    public void SetConvertedStackType(bool isConverted)
    {
        _isConvertedStack = isConverted;
    }

    public void SetValue(int value, bool isDouble = false)
    {
        var blackChips = 0;
        var whiteChips = 0;
        var redChips = 0;
        var greenChips = 0;
        var blueChips = 0;

        var remnant = 0;
        
        var tmpValue = value;

        if (value >= 500)
        {
            blackChips = value / 500;
        }

        if (value >= 100)
        {
            remnant = value - (blackChips * 500);
            
            whiteChips = remnant / 100;
        }

        if (value >= 50)
        {
            remnant = value - (blackChips * 500) - (whiteChips * 100);
            redChips = remnant / 50;
        }

        if (value >= 10)
        {
            remnant = value - (blackChips * 500) - (whiteChips * 100) - (redChips * 50);
            greenChips = remnant / 10;
        }

        if (value >= 5)
        {
            remnant = value - (blackChips * 500) - (whiteChips * 100) - (redChips * 50) - (greenChips * 10);
            blueChips = remnant / 5;
        }

        foreach (var obj in _chips)
        {
            obj.gameObject.SetActive(false);
        }

        var currentChip = 0;
        
        for (var i = 0; i < blackChips; i++)
        {
            _chips[currentChip].sprite = _chipSpriteDictionary[500];
            _chips[currentChip].gameObject.SetActive(true);
            currentChip++;
        }
        
        for (var i = 0; i < whiteChips; i++)
        {
            _chips[currentChip].sprite = _chipSpriteDictionary[100];
            _chips[currentChip].gameObject.SetActive(true);
            currentChip++;
        }
        
        for (var i = 0; i < redChips; i++)
        {
            _chips[currentChip].sprite = _chipSpriteDictionary[50];
            _chips[currentChip].gameObject.SetActive(true);
            currentChip++;
        }
        
        for (var i = 0; i < greenChips; i++)
        {
            _chips[currentChip].sprite = _chipSpriteDictionary[10];
            _chips[currentChip].gameObject.SetActive(true);
            currentChip++;
        }
        
        for (var i = 0; i < blueChips; i++)
        {
            _chips[currentChip].sprite = _chipSpriteDictionary[5];
            _chips[currentChip].gameObject.SetActive(true);
            currentChip++;
        }
        
        var activeChips = blackChips + whiteChips +  redChips + greenChips + blueChips;
        var chipsProgress = ((float)(activeChips-2) / (float)_chips.Length);
        var spacing = (-1) * Mathf.Lerp(_minSpacing, _maxSpacing, chipsProgress);
        
        _verticalLayoutGroup.spacing = spacing;
    }
}
