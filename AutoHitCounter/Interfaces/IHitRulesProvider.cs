using System;

namespace AutoHitCounter.Interfaces;

public interface IHitRulesProvider
{
    bool GetRule(string key);
    event Action OnHitRulesChanged;
}
