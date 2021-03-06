// The Skill struct only contains the dynamic skill properties, so that the
// static properties can be read from the scriptable object.
//
// Skills have to be structs in order to work with Lists.
//
// We implemented the cooldowns in a non-traditional way. Instead of counting
// and increasing the elapsed time since the last cast, we simply set the
// 'end' Time variable to Time.time + cooldown after casting each time.
// This way we don't need an extra Update method that increases the elapsed time
// for each skill all the time.
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GameJam
{
    [Serializable]
    public partial struct Skill
    {
        // hashcode used to reference the real ItemTemplate (can't link to template
        // directly because synclist only supports simple types). and syncing a
        // string's hashcode instead of the string takes WAY less bandwidth.
        public int hash;

        // dynamic stats (cooldowns etc.)
        public int level; // 0 if not learned, >0 if learned
        public double castTimeEnd; // server time. double for long term precision.
        public double cooldownEnd; // server time. double for long term precision.

        // constructors
        public Skill(ScriptableSkill data)
        {
            hash = data.name.GetStableHashCode();

            // learned only if learned by default
            level = data.learnDefault ? 1 : 0;

            // ready immediately
            castTimeEnd = cooldownEnd = Time.time;
        }

        // wrappers for easier access
        public ScriptableSkill data
        {
            get
            {
                // show a useful error message if the key can't be found
                // note: ScriptableSkill.OnValidate 'is in resource folder' check
                //       causes Unity SendMessage warnings and false positives.
                //       this solution is a lot better.
                if (!ScriptableSkill.All.ContainsKey(hash))
                {
                    throw new KeyNotFoundException("There is no ScriptableSkill with hash=" + hash + ". Make sure that all ScriptableSkills are in the Resources folder so they are loaded properly.");
                }

                return ScriptableSkill.All[hash];
            }
        }

        public string name => data.name;
        public float castTime => data.castTime.Get(level);
        public float cooldown => data.cooldown.Get(level);
        public float castRange => data.castRange.Get(level);
        public int manaCosts => data.manaCosts.Get(level);
        public Sprite icon => data.icon;
        public bool learnDefault => data.learnDefault;
        public bool showCastBar => data.showCastBar;
        public bool cancelCastIfTargetDied => data.cancelCastIfTargetDied;
        public bool canCancelCast => data.canCancelCast;
        public bool allowMovement => data.allowMovement;
        public int maxLevel => data.maxLevel;
        public int upgradeRequiredLevel => data.requiredLevel.Get(level + 1);

        // events
        public bool CheckSelf(Entity caster, bool checkSkillReady = true)
        {
            return (!checkSkillReady || IsReady()) &&
                   data.CheckSelf(caster, level);
        }
        public bool CheckTarget(Entity caster) { return data.CheckTarget(caster); }
        public bool CheckDistance(Entity caster, out Vector3 destination) { return data.CheckDistance(caster, level, out destination); }
        public bool CheckFOV(Entity caster) { return data.CheckFOV(caster, level); }
        public void Apply(Entity caster) { data.Apply(caster, level); }

        // tooltip - dynamic part
        public string ToolTip(bool showRequirements = false)
        {
            // unlearned skills (level 0) should still show tooltip for level 1
            int showLevel = Mathf.Max(level, 1);

            // note: caching StringBuilder is worse for GC because .Clear frees the internal array and reallocates.
            StringBuilder tip = new StringBuilder(data.ToolTip(showLevel, showRequirements));

            return tip.ToString();
        }

        // how much time remaining until the casttime ends? (using server time)
        public float CastTimeRemaining() => Time.time >= castTimeEnd ? 0 : (float)(castTimeEnd - Time.time);

        // we are casting a skill if the casttime remaining is > 0
        public bool IsCasting() => CastTimeRemaining() > 0;

        public void SetOnCooldown(double cooldown) => cooldownEnd = Time.time + cooldown;

        // how much time remaining until the cooldown ends? (using server time)
        public float CooldownRemaining() => Time.time >= cooldownEnd ? 0 : (float)(cooldownEnd - Time.time);

        public bool IsOnCooldown() => CooldownRemaining() > 0;

        public bool IsReady() => !IsCasting() && !IsOnCooldown();
    }
}