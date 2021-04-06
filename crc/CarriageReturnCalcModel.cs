using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.JSInterop;
using crc.lib;

public class CarriageReturnCalcModel
{
    private readonly IJSRuntime jsRuntime;

    public CarriageReturnCalcModel()
    {
        initialize();
    }

    public CarriageReturnCalcModel(IJSRuntime jsRuntime)
    {
        initialize();
        this.jsRuntime = jsRuntime;
    }

    /// <summary>
    /// 現在の入力
    /// </summary>
    public string Input { get; set; }

    /// <summary>
    /// 計算直後。計算結果が表示されている。
    /// 数字キー入力時は結果をクリアして入力
    /// </summary>
    public bool IsCalculated { get; set; }

    /// <summary>
    /// 計算でエラーが発生した場合は処理を止める
    /// </summary>
    public bool HasError { get; set; }

    public string Operator { get; set; }

    public string CurrentExpression { get; set; }

    public decimal LeftVal { get; set; }

    public decimal Result { get; set; }

    public Card CurrentCard { get; set; }

    /// <summary>
    /// メモリの履歴
    /// </summary>
    public List<Card> Cards { get; set; }

    public Trash Trash { get; set; }

    /// <summary>
    /// 数字ボタン押下
    /// </summary>
    public void AddNum( string numChar)
    {
        if (IsCalculated) {
            Input = numChar;
            IsCalculated = false;
        }
        else if (Input.Contains("e"))
        {
            jsRuntime.InvokeAsync<bool>("alert", "桁数オーバー！数字");
            IsCalculated = true;
        }
        else
        if (Input == "0")
        {
            Input = numChar;
        } else if(Input.Length < 16){
            Input = Input + numChar;
        }   // 17桁以上の時は何もしない
    }

    public void AddDecimalPoint()
    {
        if (!Input.Contains(".") && Input.Length < 15)
        {
            Input = Input + ".";
        }
    }

    public void Operation(string s)
    {
        string tempExpression = CurrentExpression + Input + " " + s + " ";
        Culc();
        if (!HasError)
        {
            CurrentExpression = tempExpression;
            Operator = s;
        }
        else
        {
            HasError = false;
        }
    }

    public void ToggleSign()
    {
        decimal newInput = 0m - ConvUtil.String2Decimal(Input);
        Input = ConvUtil.Decimal2String(newInput);
    }

    public void RemoveCard(Card card)
    {
        Cards.Remove(card);
    }

    public void Switch2CurrentCard(Card card)
    {
        // 現在のカードがあればヒストリーへ
        if (CurrentCard.Lines.Count > 0)
        {
            // 新しいカードに現在のカードの行をコピー
            Card newCard = new Card(CurrentCard.Lines);

            // コピーしたカードを追加
            Cards.Add(newCard);
        }
        Cards.Remove(card);
        CurrentCard = card;
    }

    /// <summary>
    /// 計算
    /// </summary>
    public void Culc()
    {
        decimal left = LeftVal;
        decimal right = ConvUtil.String2Decimal(Input);
        decimal zero = 0m;

        switch (Operator) {
            case "＋":
                LeftVal = left + right;
                break;
            case "－":
                LeftVal = left - right;
                break;
            case "×":
                try
                {
                    LeftVal = left * right;
                }
                catch
                {
                    HasError = true;
                    jsRuntime.InvokeAsync<bool>("alert", "桁数オーバー！×");
                }
                break;
            case "÷":
                if (right == zero)
                {
                    HasError = true;
                }
                else
                {
                    LeftVal = Math.Round( left / right, 15);
                }
                break;
        }

        if (HasError)
        {
            IsCalculated = false;    // クリアしない
        }
        else
        {
            Input = ConvUtil.Decimal2String(LeftVal);
            IsCalculated = true;
        }
    }

    /// <summary>
    /// =を押した時
    /// </summary>
    public void Grandtotal()
    {
        Decimal total = 0m;
        // 入力中の値、式の処理

        // CurrentCard　の合計
        total += CurrentCard.GetTotal();

        Input = ConvUtil.Decimal2String(total);
        IsCalculated = true;
    }

    /// <summary>
    /// 改行
    /// </summary>
    public void AddLine(string sign)
    {
        if(Input.Length > 0 && Input != ".")
        {
            var tempExpression = CurrentExpression + Input;
            Culc();
            if (!HasError)
            {
                Line newLine = new Line();
                newLine.Sign = sign;
                newLine.Expression = tempExpression;
                newLine.Result = LeftVal;
                CurrentCard.Lines.Add(newLine);

                //initializePad();
                Input = ConvUtil.Decimal2String(LeftVal);
                initializeExpression();
                IsCalculated = true;    // 上書き
            }
            else
            {
                HasError = false;
            }
        }
    }

    /// <summary>
    /// 一文字消す（BSキー）
    /// </summary>
    public void ClearChar()
    {
        if (Input.Length == 1 || Input.Contains("e"))
        {
            Input = "0";
        }
        else
        {
            var s = Input.Substring(0, Input.Length - 1);
            if (s == "-")
            {
                Input = "0";
            }
            else
            {
                Input = s;
            }
        }
    }

    /// <summary>
    /// 現在の入力をクリア
    /// </summary>
    public void ClearEntry()
    {
        Input = "0";
    }

    /// <summary>
    /// 
    /// </summary>
    public void ClearExpression()
    {
        initializeExpression();
    }

    /// <summary>
    /// 新たなカード作成
    /// </summary>
    public void ClearMemory()
    {
        // コピーするデータが無ければ新たなカードを作らない
        if (CurrentCard.Lines.Count > 0)
        {
            // 新しいカードに現在のカードの行をコピー
            Card newCard = new Card(CurrentCard.Lines);

            // コピーしたカードを追加
            Cards.Add(newCard);

            // 現在のカードの行を削除
            CurrentCard = new Card();
        }
    }

    /// <summary>
    /// カード（履歴）、ゴミ箱、全て削除
    /// </summary>
    public void ClearAll()
    {
        initialize();
    }

    private void initializeExpression()
    {
        IsCalculated = false;
        Operator = "＋";
        CurrentExpression = string.Empty;
        LeftVal = 0m;
        Result = 0m;
        Input = (HasError) ? "0" : Input;
        HasError = false;
    }

    private void initializePad()
    {
        Input = "0";
        initializeExpression();
    }

    private void initialize()
    {
        initializePad();
        CurrentCard = new Card();
        Cards = new List<Card>();
        Trash = new Trash();
    }
}

public class Line
{
    /// <summary>
    /// 符号
    /// </summary>
    public string Sign { get; set; }

    /// <summary>
    /// 単なる文字列（計算履歴）
    /// </summary>
    public string Expression { get; set; }

    /// <summary>
    /// 計算結果
    /// </summary>
    public decimal Result { get; set; }

    public void ToggleSign()
    {
        Sign = (Sign == "＋") ? "－" : "＋";
    }
}

public class Card
{
    public Card()
    {
        Lines = new List<Line>();
    }

    public Card(List<Line> lines)
    {
        Lines = new List<Line>();

        foreach(var line in lines)
        {
            Lines.Add(line);
        }
    }


    /// <summary>
    /// カード上の行（式の集まり）
    /// </summary>
    public List<Line> Lines { get; set; }

    //public void RemoveAtLine(int idx)
    //{
    //    Lines.RemoveAt(idx);
    //}

    public void RemoveLine(Line line)
    {
        Lines.Remove(line);
    }

    public Decimal GetTotal()
    {
        Decimal sum = 0m;

        foreach (var line in Lines)
        {
            if (line.Sign == "＋")
            {
                sum = sum + line.Result;
            } else
            {
                sum = sum - line.Result;
            }
        }
        return sum;
    }
}

public class Trash
{
    public Trash()
    {
        Lines = new List<Line>();
    }
    public List<Line> Lines { get; set; }
}
