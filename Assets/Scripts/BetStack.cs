using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BetStack : MonoBehaviour
{
    [SerializeField] private VerticalLayoutGroup _verticalLayoutGroup;
    [SerializeField] private Image[] _chips;
    [SerializeField] private Sprite[] _chipSprites;
    
    [SerializeField] private int _activeChips = 1;
    
    private int _oldActiveChips;
    private readonly int _maxStackShips = 10;

    private bool _isConvertedStack = false;
    
    List<float> _chipSpacings = new List<float>();
    Dictionary<int, Sprite> _chipSpriteDictionary;

    private void Awake()
    {
        _oldActiveChips = _activeChips;
        
        _chipSpacings =  new List<float>()
        {
            -208f,
            -208f,
            -208f,
            -188.8f,
            -168.48f,
            -137.6f,
            -123.6f,
            -109.6f,
            -95.6f,
            -81.6f,
            -68f
        };

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

    public int SetValue(int value, bool isDouble = false)
    {
        var chipsValueLeft = 0;
        
        var blackChips = 0;
        var whiteChips = 0;
        var redChips = 0;
        var greenChips = 0;
        var blueChips = 0;

        var remnant = 0;
        
        var tmpValue = isDouble && !_isConvertedStack ? value / 2 : value;

        if (tmpValue >= 500)
        {
            blackChips = tmpValue / 500;
        }

        if (tmpValue >= 100)
        {
            remnant = tmpValue - (blackChips * 500);
            
            whiteChips = remnant / 100;
        }

        if (tmpValue >= 50)
        {
            remnant = tmpValue - (blackChips * 500) - (whiteChips * 100);
            redChips = remnant / 50;
        }

        if (tmpValue >= 10)
        {
            remnant = tmpValue - (blackChips * 500) - (whiteChips * 100) - (redChips * 50);
            greenChips = remnant / 10;
        }

        if (tmpValue >= 5)
        {
            remnant = tmpValue - (blackChips * 500) - (whiteChips * 100) - (redChips * 50) - (greenChips * 10);
            blueChips = remnant / 5;
        }

        if (isDouble && !_isConvertedStack)
        {
            blackChips *= 2;
            whiteChips *= 2;
            redChips *= 2;
            greenChips *= 2;
            blueChips *= 2;
        }

        //ResetStack();

        var currentChip = 0;
        var tmpBlackChips = blackChips;
        var tmpWhiteChips = whiteChips;
        var tmpRedChips = redChips;
        var tmpGreenChips = greenChips;
        var tmpBlueChips = blueChips;
        
        if(currentChip < _maxStackShips)
        {
            for (var i = 0; i < blackChips; i++)
            {
                _chips[currentChip].sprite = _chipSpriteDictionary[500];
                _chips[currentChip].gameObject.SetActive(true);
                currentChip++;
                tmpBlackChips--;

                if (currentChip >= _maxStackShips)
                {
                    break;
                }
            }
        }
        
        if(currentChip < _maxStackShips)
        {
            for (var i = 0; i < whiteChips; i++)
            {
                _chips[currentChip].sprite = _chipSpriteDictionary[100];
                _chips[currentChip].gameObject.SetActive(true);
                currentChip++;
                tmpWhiteChips--;

                if (currentChip >= _maxStackShips)
                {
                    break;
                }
            }
        }
        
        if(currentChip < _maxStackShips)
        {
            for (var i = 0; i < redChips; i++)
            {
                _chips[currentChip].sprite = _chipSpriteDictionary[50];
                _chips[currentChip].gameObject.SetActive(true);
                currentChip++;
                tmpRedChips--;

                if (currentChip >= _maxStackShips)
                {
                    break;
                }
            }
        }
        
        if(currentChip < _maxStackShips)
        {
            for (var i = 0; i < greenChips; i++)
            {
                _chips[currentChip].sprite = _chipSpriteDictionary[10];
                _chips[currentChip].gameObject.SetActive(true);
                currentChip++;
                tmpGreenChips--;

                if (currentChip >= _maxStackShips)
                {
                    break;
                }
            }
        }
        
        if(currentChip < _maxStackShips)
        {
            for (var i = 0; i < blueChips; i++)
            {
                _chips[currentChip].sprite = _chipSpriteDictionary[5];
                _chips[currentChip].gameObject.SetActive(true);
                currentChip++;
                tmpBlueChips--;

                if (currentChip >= _maxStackShips)
                {
                    break;
                }
            }
        }

        var totalChips = blackChips + whiteChips + redChips + greenChips + blueChips;
        SetChipsSpacing(totalChips);
        
        chipsValueLeft = (tmpBlackChips * 500) +  (tmpWhiteChips * 100) + (tmpRedChips * 50) + (tmpGreenChips * 10) + (tmpBlueChips * 5);

        return chipsValueLeft;
    }

    private void SetChipsSpacing(int totalChips)
    {
        totalChips = Mathf.Clamp(totalChips, 0, _maxStackShips);

        var spacing = _chipSpacings[totalChips];
        
        _verticalLayoutGroup.spacing = spacing;
    }

    public void ResetStack()
    {
        foreach (var obj in _chips)
        {
            obj.gameObject.SetActive(false);
        }
    }
}
