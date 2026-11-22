using System.Collections.Generic;

public static class Loc
{
    static readonly Dictionary<string, string> English = new Dictionary<string, string>
    {
        { "menu.title", "The Last Healer" },
        { "menu.play", "Play" },
        { "menu.options", "Options" },
        { "menu.quit", "Quit" },
        { "menu.back", "Back" },
        { "options.language", "Language" },
        { "options.english", "English" },
        { "options.german", "Deutsch" },
        { "options.sfx", "Game sounds" },
        { "prepare.title", "Prepare" },
        { "prepare.heroes", "Heroes" },
        { "hero.fighter", "Fighter" },
        { "hero.ranger", "Ranger" },
        { "prepare.cleric", "Cleric" },
        { "prepare.start", "Start battle" },
        { "prepare.party", "Party {0}/{1}" },
        { "prepare.bring", "Bring" },
        { "prepare.bench", "Bench" },
        { "hero.state.locked", "Locked" },
        { "hero.state.unlocked", "Unlocked" },
        { "hero.state.active", "Active" },
        { "prepare.empty", "Empty" },
        { "prepare.points", "Points: {0}" },
        { "prepare.unlock", "Unlock" },
        { "prepare.spells", "Spells" },
        { "hotbar.empty", "Empty" },
        { "hotbar.ready", "ready" },
        { "hotbar.waiting", "queued" },
        { "spell.PeriodicHeal", "Periodic Heal" },
        { "spell.BurstHeal", "Burst" },
        { "spell.Shield", "Shield" },
        { "skill.fighter.core", "Brace" },
        { "skill.fighter.core.body", "Holds the line." },
        { "skill.fighter.guard", "Guard" },
        { "skill.fighter.guard.body", "A harder hit." },
        { "skill.fighter.step", "Step" },
        { "skill.fighter.step.body", "Closes a little faster." },
        { "skill.fighter.reach", "Reach" },
        { "skill.fighter.reach.body", "A wider swing." },
        { "skill.fighter.oath", "Oath" },
        { "skill.fighter.oath.body", "Hidden until the oath is kept." },
        { "skill.ranger.core", "Focus" },
        { "skill.ranger.core.body", "Steady aim." },
        { "skill.ranger.aim", "Longshot" },
        { "skill.ranger.aim.body", "Arrows travel farther." },
        { "skill.cleric.core", "Presence" },
        { "skill.cleric.core.body", "The cleric stands with the party." },
        { "skill.cleric.ward", "Ward" },
        { "skill.cleric.ward.body", "A stronger shield, later." }
    };

    static readonly Dictionary<string, string> German = new Dictionary<string, string>
    {
        { "menu.title", "Der letzte Heiler" },
        { "menu.play", "Spielen" },
        { "menu.options", "Optionen" },
        { "menu.quit", "Beenden" },
        { "menu.back", "Zurück" },
        { "options.language", "Sprache" },
        { "options.english", "English" },
        { "options.german", "Deutsch" },
        { "options.sfx", "Spielgeräusche" },
        { "prepare.title", "Vorbereitung" },
        { "prepare.heroes", "Helden" },
        { "hero.fighter", "Kämpfer" },
        { "hero.ranger", "Waldläufer" },
        { "prepare.cleric", "Kleriker" },
        { "prepare.start", "Schlacht beginnen" },
        { "prepare.party", "Gruppe {0}/{1}" },
        { "prepare.bring", "Mitnehmen" },
        { "prepare.bench", "Absetzen" },
        { "hero.state.locked", "Gesperrt" },
        { "hero.state.unlocked", "Freigeschaltet" },
        { "hero.state.active", "Aktiv" },
        { "prepare.empty", "Leer" },
        { "prepare.points", "Punkte: {0}" },
        { "prepare.unlock", "Freischalten" },
        { "prepare.spells", "Zauber" },
        { "hotbar.empty", "Leer" },
        { "hotbar.ready", "bereit" },
        { "hotbar.waiting", "wartet" },
        { "spell.PeriodicHeal", "Heilung" },
        { "spell.BurstHeal", "Stoßheilung" },
        { "spell.Shield", "Schild" },
        { "skill.fighter.core", "Stand" },
        { "skill.fighter.core.body", "Hält die Linie." },
        { "skill.fighter.guard", "Deckung" },
        { "skill.fighter.guard.body", "Ein härterer Schlag." },
        { "skill.fighter.step", "Schritt" },
        { "skill.fighter.step.body", "Geht etwas schneller ran." },
        { "skill.fighter.reach", "Reichweite" },
        { "skill.fighter.reach.body", "Ein weiterer Schwung." },
        { "skill.fighter.oath", "Eid" },
        { "skill.fighter.oath.body", "Bleibt verborgen, bis der Eid erfüllt ist." },
        { "skill.ranger.core", "Fokus" },
        { "skill.ranger.core.body", "Ruhiges Zielen." },
        { "skill.ranger.aim", "Weitschuss" },
        { "skill.ranger.aim.body", "Pfeile fliegen weiter." },
        { "skill.cleric.core", "Präsenz" },
        { "skill.cleric.core.body", "Der Kleriker steht bei der Gruppe." },
        { "skill.cleric.ward", "Schutz" },
        { "skill.cleric.ward.body", "Später ein stärkeres Schild." }
    };

    public static string Text(string key)
    {
        Dictionary<string, string> table = GameSettings.Language == "de" ? German : English;
        string value;
        if (table.TryGetValue(key, out value))
            return value;
        if (English.TryGetValue(key, out value))
            return value;
        return key;
    }

    public static string Format(string key, params object[] args)
    {
        return string.Format(Text(key), args);
    }
}
