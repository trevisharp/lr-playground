using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;

using Pamella;

App.Open<MainView>();

public class MainView : View
{
    SyntacticRules syn = new SyntacticRules();
    LRParser parser = null;

    protected override void OnStart(IGraphics g)
    {
        g.SubscribeKeyDownEvent(key =>
        {
            if (key == Input.Escape)
                App.Close();
        });

        syn.Add('S', 'E');
        syn.Add('E', 'B');
        syn.Add('B', '1');
        syn.Add('B', '0');

        parser = syn.GetParser(0, false);
    }

    protected override void OnRender(IGraphics g)
    {
        g.Clear(Color.White);
        
        var y = 10;
        foreach (var rule in syn.Rules)
        {
            var rect = new RectangleF(5, y, 90, 20);
            g.DrawRectangle(rect, Pens.Black);
            var prod = rule.prd
                .Aggregate("", (a, c) => a += " " + c);
            g.DrawText(rect,
                $"{rule.lft} -> {prod}"
            );
            y += 30;
        }

        if (parser is null)
            return;
        
        parser.Draw(100, g);
    }
}

public class SyntacticRules
{
    public List<(char lft, char[] prd)> Rules { get; private set; } = new(); 

    public void Add(char lft, params char[] prd)
        => this.Rules.Add((lft, prd));

    public LRParser GetParser(int k, bool isLA)
    {
        if (!isLA && k == 0)
            return new LR0(this);
        
        throw new NotImplementedException();
    }
}

public abstract class LRParser
{
    public List<char> Columns { get; private set; } = new();
    public List<char> Rows { get; private set; } = new();
    public Dictionary<(int, int), string> Table { get; private set; } = new();

    public abstract void Draw(float x, IGraphics g);
}

public class LR0 : LRParser
{
    private List<List<(char lft, char[] prd, int dot)>> states = new();
    
    public LR0(SyntacticRules sr)
    {
        Columns.AddRange(
            sr.Rules
            .Select(r => r.lft)
            .Concat(
                sr.Rules.SelectMany(r => r.prd)
            )
            .Distinct()
            .Where(k => k != 'S')
            .Append('$')
            .OrderBy(k => k switch {
                >= 'A' and <= 'Z' => char.MaxValue,
                '$' => char.MaxValue - 1,
                _ => k
            })
        );

        var initial = sr.Rules.FirstOrDefault(r => r.lft == 'S');

        var state0 = new List<(char lft, char[] prd, int dot)>();
        state0.Add(('S', initial.prd, 0));
        closure(state0);
        this.states.Add(state0);

        int stn = 0;
        foreach (var state in states)
        {
            Rows.Add((char)(stn - '0'));
            stn++;
        }

        void expand(List<(char lft, char[] prd, int dot)> state)
        {
            foreach (var input in Columns)
            {
                
            }
        }

        void closure(List<(char lft, char[] prd, int dot)> state)
        {
            for (int i = 0; i < state.Count; i++)
            {
                addClosure(state, state[i]);
            }
        }

        void addClosure(
            List<(char lft, char[] prd, int dot)> state,
            (char lft, char[] prd, int dot) sbrule)
        {
            var key = sbrule.prd[sbrule.dot];
            if (key < 'A' || key > 'Z')
                return;
            
            foreach (var rule in sr.Rules)
            {
                if (rule.lft != key)
                    continue;
                
                state.Add((key, rule.prd, 0));
            }
        }
    }

    public override void Draw(float x, IGraphics g)
    {
        float y = 10;
        
        int i = 0;
        foreach (var state in this.states)
        {
            var rect = new RectangleF(x, y, 20, 20);
            g.DrawRectangle(rect, Pens.Black);
            g.DrawText(rect, i.ToString());
            x += 20;
            
            foreach (var rule in state)
            {
                rect = new RectangleF(x, y, 100, 20);
                g.DrawRectangle(rect, Pens.Black);
                
                var prod = rule.prd
                    .Select((x, i) => 
                        i == rule.dot ?
                        "." + x.ToString() :
                        x.ToString()
                    )
                    .Aggregate("", (a, c) => a += " " + c);
                g.DrawText(rect,
                    $"{rule.lft} -> {prod}"
                );
                y += 20;
            }
            y += 5;
        }

        x += 105;
        y = 10;
        foreach (var column in this.Columns)
        {
            var rect = new RectangleF(x, y, 40, 20);
            g.DrawRectangle(rect, Pens.Black);
            g.DrawText(rect, column.ToString());
            x += 40;
        }
    }
}