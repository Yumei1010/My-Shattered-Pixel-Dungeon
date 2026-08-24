# 04 - 角色与战斗系统

> 核心文件：`Char.java`, `Hero.java`, `Mob.java`（部分）

## 属性

```java
Char (abstract):
  pos: int           // 地图位置
  HT: int            // 最大生命值
  HP: int            // 当前生命值
  baseSpeed: float   // 基础速度 (=1)
  viewDistance: int  // 视野距离 (=8)
  paralysed: int     // 麻痹剩余回合
  rooted: boolean    // 定身
  flying: boolean    // 飞行
  invisible: int     // 隐身回合
  alignment: enum    // ENEMY / NEUTRAL / ALLY
  buffs: LinkedHashSet<Buff>  // 状态容器
  fieldOfView: boolean[]     // 视野数组
  resistances: HashSet<Class>// 抵抗效果
  immunities: HashSet<Class> // 免疫效果
  properties: HashSet<Property> // 属性标签

Hero 额外:
  heroClass: HeroClass    // 职业
  subClass: HeroSubClass  // 子职业
  armorAbility: ArmorAbility // 护甲技能
  STR: int                // 力量
  lvl: int                // 等级
  exp: int                // 经验
  belongings: Belongings  // 背包容器
  talents: 天赋树
  curAction: HeroAction   // 当前动作
  resting: boolean        // 是否休息中
```

## 命中判定（Char.hit）

```java
static boolean hit(attacker, defender, accMulti, magic):
  acuStat = attacker.attackSkill(defender)
  defStat = defender.defenseSkill(attacker)

  // 隐身攻击（偷袭）必中
  if (attacker.invisible > 0 && attacker.canSurpriseAttack()) acuStat = INFINITE_ACCURACY

  // 专注buff（和尚）闪避无限
  if (defender.buff(Focus)) defStat = INFINITE_EVASION

  // 无限闪避 > 无限命中
  if (defStat >= INFINITE_EVASION) return false
  if (acuStat >= INFINITE_ACCURACY) return true

  // 随机掷骰
  acuRoll = Random.Float(acuStat) × 祝福修正 × 虚弱修正 × 眩晕修正 × 冠军修正 × 飞升修正
  defRoll = Random.Float(defStat) × 祝福修正 × 虚弱修正 × 眩晕修正 × 冠军修正 × 飞升修正

  return acuRoll >= defRoll
```

## 攻击流程（Char.attack）

```java
boolean attack(enemy, dmgMulti, dmgBonus, accMulti):
  if (enemy.isInvulnerable) → 显示"免疫" → 返回 false
  if (命中):
    dr = enemy.drRoll()  // 护甲减伤
    dmg = damageRoll()   // 伤害骰
    dmg *= dmgMulti + dmgBonus
    狂暴修正 → 狂怒修正 → 各种增益修正 → 飞升修正
    effectiveDamage = enemy.defenseProc(this, dmg)  // 防御前处理
    effectiveDamage = max(effectiveDamage - dr, 0)  // 减伤
    effectiveDamage = attackProc(enemy, effectiveDamage)  // 攻击后处理
    enemy.damage(effectiveDamage, this)
    return true
  else:
    → 显示闪避
    return false
```

## 伤害公式

```java
damage(dmg, src):
  // 生命链接分摊伤害
  // 脆弱(Vulnerable): +33%
  // 厄运(Doom): +67%
  // 死亡标记: +25%
  // 元素抗性: 每层抗性 -50%
  // 元素免疫: 0
  // 冠军抗性: 按比例递减
  // 护甲减伤 (AntiMagic)
  // 护盾吸收 (ShieldBuff)
  HP -= 最终伤害
  if (HP <= 0) die(src)
```

## 防御减伤（drRoll）

```java
int drRoll():
  dr = Random.NormalIntRange(0, Barkskin.currentLevel(this))
  return dr
```

**护甲减伤修正：** 实际护甲值通过 `glyphLevel` 查询，武器的减伤通过 `Weapon.drRoll` 和 `Armor.drRoll` 实现（在各自的子类中）。

## 速度系统

```java
float speed():
  speed = baseSpeed  // 默认 1
  if (Cripple) speed /= 2
  if (Stamina) speed *= 1.5
  if (Adrenaline) speed *= 2
  if (Haste) speed *= 3
  if (Dread) speed *= 2
  // 护甲雕纹修正
  speed *= Swiftness.speedBoost
  speed *= Flow.speedBoost
  speed *= Bulk.speedBoost
```

## 突袭判定（surprisedBy）

```java
boolean surprisedBy(enemy, attacking):
  return enemy == Dungeon.hero
    && (enemy.invisible > 0  // 隐身
        || !enemySeen         // 未发现
        || !fieldOfView[enemy.pos])  // 视野外
    && enemy.canSurpriseAttack()
```

## 属性标签（Property）

```java
Property:
  BOSS       → 免疫 Grim/恐惧/盟友buff
  MINIBOSS   → 免疫恐惧/盟友buff
  BOSS_MINION→ 无特殊
  UNDEAD     → 无特殊
  DEMONIC    → 无特殊
  INORGANIC  → 免疫流血/毒气/中毒
  FIERY      → 抵抗火焰伤害，免疫燃烧
  ICY        → 抵抗冰冻伤害，免疫冰冻/寒冷
  ACIDIC     → 抵抗腐蚀，免疫粘液
  ELECTRIC   → 抵抗闪电伤害
  LARGE      → 需要 2×2 空间
  IMMOVABLE  → 不可移动，免疫眩晕
  STATIC     → 免疫绝大多数控制效果
```

## 英雄属性（Hero）

```java
public static final int MAX_LEVEL = 30;
public static final int STARTING_STR = 10;
```

**升级经验：** `exp = lvl^2 + 5`（每级所需经验递增）

**力量收益：** 每升 2 级 +1 STR，起始 10，最高 18（可用药水提升）

**HP 成长：** 每级 +5 HP（Warrior 额外 +2，Mage 额外 +1）

**英雄各职业初始属性：**
```
Warrior:   HP=30, STR=11, 起始装备: 短剑+布甲+1口粮
Mage:      HP=20, STR=10, 起始装备: 魔法飞弹法杖+1口粮
Rogue:     HP=25, STR=10, 起始装备: 匕首+布甲+1口粮
Huntress:  HP=22, STR=10, 起始装备: 短弓+布甲+1口粮
Duelist:   HP=25, STR=11, 起始装备: 细剑+布甲+1口粮
Cleric:    HP=22, STR=10, 起始装备: 钉头锤+布甲+圣书+1口粮
```