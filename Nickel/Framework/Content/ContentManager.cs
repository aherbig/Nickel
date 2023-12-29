using System;
using Microsoft.Extensions.Logging;

namespace Nickel;

internal sealed class ContentManager
{
    public SpriteManager Sprites { get; private init; }
    public DeckManager Decks { get; }
    public StatusManager Statuses { get; }
    public CardManager Cards { get; }
    public ArtifactManager Artifacts { get; }
    public CharacterManager Characters { get; }

    public ContentManager(
        SpriteManager sprites,
        DeckManager decks,
        StatusManager statuses,
        CardManager cards,
        ArtifactManager artifacts,
        CharacterManager characters
    )
    {
        this.Sprites = sprites;
        this.Decks = decks;
        this.Statuses = statuses;
        this.Cards = cards;
        this.Artifacts = artifacts;
        this.Characters = characters;
    }

    public ContentManager(Func<ModLoadPhase> currentModLoadPhaseProvider, Func<IModManifest, ILogger> loggerProvider) : this(
        new SpriteManager(),
        new DeckManager(currentModLoadPhaseProvider),
        new StatusManager(currentModLoadPhaseProvider),
        new CardManager(currentModLoadPhaseProvider),
        new ArtifactManager(currentModLoadPhaseProvider),
        new CharacterManager(currentModLoadPhaseProvider, loggerProvider)
    ) { }

    internal void InjectQueuedEntries()
    {
        this.Decks.InjectQueuedEntries();
        this.Statuses.InjectQueuedEntries();
        this.Cards.InjectQueuedEntries();
        this.Artifacts.InjectQueuedEntries();
        this.Characters.InjectQueuedEntries();
    }
}
