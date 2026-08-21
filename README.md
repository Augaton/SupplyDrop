# SupplyDrop 4.3

> Portage EXILED 9.14.2 d'un plugin de **Wafel, KadeDev, JesusQC**. Depot non
> affilie aux auteurs d'origine. Voir [NOTICE.md](NOTICE.md) pour l'attribution.

Largages de ravitaillement periodiques pour le MTF et l'Insurrection du Chaos.

**EXILED 9.14.2** — `dotnet build -c Release SupplyDrop/SupplyDrop.csproj`

## Profils

Les largages sont **pilotes par les donnees**. Chaque entree de `profiles` decrit
un type de largage complet : contenu, horaires, positions, annonce, balise.
Ajouter une entree avec une cle inutilisee cree un nouveau type de largage sans
recompiler.

Deux profils par defaut : `mtf` et `chaos`. Les deux embarquent **deux pieces**
depuis la 4.3 : BetterCoinflipsRewritten detruit la piece a l'usage, les
largages sont donc une source de reappro.

| Cle | Role |
|---|---|
| `first_drop_seconds` | Delai avant le premier largage, depuis le debut du round |
| `interval_seconds` | Delai entre deux largages du meme profil |
| `announcement_lead_seconds` | Temps entre l'annonce et l'apparition des objets |
| `max_drops_per_round` | Plafond par round. `-1` = illimite |
| `fallback_role` | Role dont le point d'apparition sert d'ancre au largage |
| `ammo/armor/item/weapon_position` | Coordonnees fixes par categorie. `0,0,0` = ancre du role de repli |
| `scatter_radius` | Dispersion horizontale autour du point, en metres |
| `enable_beacon` | Halo lumineux sur la zone de largage |
| `custom_items` | Objets personnalises ajoutes au largage. Voir plus bas |
| `cassie_announcement` | Annonce C.A.S.S.I.E. Vide = desactivee |

## Commandes

| Commande | Permission | Effet |
|---|---|---|
| `supplydrop list` | `supplydrop.call` | Liste les profils, leurs horaires et leur etat |
| `supplydrop call <profil>` | `supplydrop.call` | Declenche un largage immediatement, avec annonce |
| `supplydrop call <profil> silencieux` | `supplydrop.call` | Largage sans annonce ni C.A.S.S.I.E |
| `supplydrop call <profil> ici` | `supplydrop.call` | Largage aux pieds de l'appelant, pour verifier le contenu |

Alias `sd`. Les modificateurs se cumulent : `sd call mtf silencieux ici`. Chaque
appel est trace avec l'auteur et la position forcee eventuelle.

## Objets personnalises (compatibilite optionnelle)

Chaque profil peut ajouter au largage des objets enregistres par un autre plugin
via **Exiled.CustomItems** : les SCP-500 modifies de
[SCP500s](https://github.com/Augaton/SCP500s), mais aussi n'importe quel
`CustomItem` d'un plugin tiers.

```yaml
custom_items:
  is_enabled: true
  draws: 2              # nombre de tirages par largage
  chance: 60            # chance en pourcent que chaque tirage aboutisse
  allow_duplicates: false
  pool:
    - reference: SCP500-Blindage
      weight: 15
    - reference: SCP500-Chirurgien
      weight: 15
```

`reference` accepte le **nom affiche** de l'objet (`SCP500-Blindage`) ou son
**identifiant numerique** (`18`). Le nom est celui enregistre par le plugin
proprietaire : pour SCP500s, c'est `display_name`, pas la cle interne.

Les deux profils par defaut arrivent avec un pool oriente : soin et equipement
pour le MTF, mobilite et agressivite pour l'Insurrection. Avec `draws: 2` et
`chance: 60`, un largage contient en moyenne **une pilule et quart**.

### La dependance reste optionnelle

Rien n'est requis a l'installation. `Exiled.CustomItems` n'est sonde qu'au
premier besoin, et tout le code typé qui le touche vit dans
`API/CustomItemBridge.cs`, isole du reste :

- assembly absente → le pool est ignore en silence, le largage classique
  fonctionne normalement ;
- assembly presente mais une reference du pool n'existe pas → avertissement
  **une seule fois par round**, l'entree est ecartee du tirage ;
- la sonde est reevaluee a chaque debut de round, donc un `reload` qui ajoute
  SCP500s est pris en compte sans redemarrage.

`supplydrop list` affiche l'etat resolu du pool de chaque profil et nomme les
references introuvables. C'est le moyen le plus rapide de verifier qu'un nom de
pilule est correctement orthographie.

## Position du largage

Le largage n'utilise **aucune coordonnee en dur**. L'ancre est le point
d'apparition reel du role indique par `fallback_role` : le MTF se pose la ou
arrive une vague MTF, l'Insurrection la ou arrive son fourgon. Ces points sont
lus dans la scene chargee, ils suivent donc les remaniements de carte de
Northwood sans intervention.

Ordre de resolution de l'ancre :

1. `fallback_role.GetRandomSpawnLocation()`
2. `SpawnLocationType.InsideSurfaceNuke`
3. `RoomType.Surface`

Si les trois echouent, le largage est **annule avec un `Log.Error`** plutot que
depose en `0,0,0`, c'est-a-dire dans le vide sous la carte.

Renseigner `ammo/armor/item/weapon_position` force des coordonnees fixes pour la
categorie concernee et court-circuite l'ancre. A n'utiliser que pour un point
precis verifie en jeu : une coordonnee obsolete envoie le contenu hors de la
carte sans aucun message d'erreur, puisque la valeur est syntaxiquement valide.

`spawn_height_offset` (config globale, 0,5 m par defaut) souleve les objets pour
qu'ils ne traversent pas le sol au moment de l'apparition.

## Dependances

Ce plugin depend de **AugatonLib**, la bibliotheque partagee de la
collection.

| Fichier | Destination |
|---|---|
| `SupplyDrop.dll` | `Plugins/7777/` |
| `AugatonLib.dll` | `Plugins/dependencies/` |
| HintServiceMeow | `Plugins/7777/` |

`AugatonLib.dll` ne va **jamais** dans `Plugins/7777/` : EXILED
tenterait de le charger comme plugin. Il doit etre deploye avant ce plugin et
mis a jour en meme temps.

Pour compiler ce depot isolement, cloner
[AugatonLib](https://github.com/Augaton/AugatonLib) a cote,
ou passer `-p:CommonProject=chemin/vers/AugatonLib.csproj`.

## Installation depuis une release

Chaque tag `v*` declenche une release qui publie une archive **contenant deja
AugatonLib**. Extraire `SupplyDrop.zip` dans `.config/EXILED/` :

```
Plugins/7777/SupplyDrop.dll
Plugins/dependencies/AugatonLib.dll
```

Les DLL sont aussi publiees separement pour une mise a jour ciblee.

Si plusieurs plugins de la collection sont installes, garder la version
d'AugatonLib la plus recente : elle est partagee par tous.

## Integration continue

`build` et `release` sont deux **workflows distincts**.

| Workflow | Declencheur | Produit |
|---|---|---|
| `build` | push sur `main`, pull request | Compile, cree le tag si besoin, declenche la release |
| `release` | tag `v*`, ou lancement manuel | Une **release** avec la DLL et l'archive |

### Publication automatique

Le job `tag` de `build` lit la balise `<Version>` du `.csproj`. Si le tag
`v<version>` n'existe pas encore, il le cree et declenche `release`.

Publier revient donc a **incrementer `<Version>` dans le `.csproj`** puis
pousser sur `main`. Sans changement de version, aucun tag n'est cree et aucune
release n'est publiee : les commits de correction ne generent pas de bruit.

La version affichee par `status` en jeu est lue dans l'assembly, elle-meme
issue de cette meme balise. Le tag, la DLL et l'affichage en jeu ne peuvent
donc pas diverger.

### Publication manuelle

```bash
git tag v1.0.0 && git push origin v1.0.0
```

Ou onglet Actions, workflow `release`, bouton **Run workflow** : le tag est cree
s'il n'existe pas.

La CI recupere AugatonLib par `actions/checkout` sur le depot
[Augaton/AugatonLib](https://github.com/Augaton/AugatonLib), branche `main` par
defaut. Le declenchement manuel de `release` permet de fixer une autre version
via l'entree `augatonlib_ref`.

Gitleaks scanne l'historique complet a chaque push et bloque en cas de secret
detecte. Il est invoque en binaire plutot que via son action GitHub : l'action
calcule une plage de commits `<precedent>^..<actuel>` qui echoue sur le commit
initial d'un depot.

## Note de version 4.2

- Compatibilite optionnelle avec les `CustomItem` d'EXILED, donc avec les
  SCP-500 modifies de SCP500s. Tirage pondere, sans doublon par defaut.
- `supplydrop list` diagnostique le pool d'objets personnalises.
- Un profil peut n'avoir que des objets personnalises, sans `items`.

## Note de version 4.1

- Les coordonnees en dur des deux profils par defaut, heritees de la 3.x, sont
  supprimees. Elles designaient des points de la surface d'une version anterieure
  du jeu et prenaient le pas sur le point de repli, qui n'etait donc jamais
  utilise : tout le contenu partait hors de la carte, balise comprise, sans la
  moindre erreur en console.
- Un largage sans position exploitable echoue desormais bruyamment.
- `spawn_height_offset` evite le clipping a travers le sol.
- `supplydrop call <profil> ici` permet de verifier un profil sans se deplacer.
- L'helicoptere du profil `mtf` est nomme « helicoptere MTF ».

## Note de portage

La version 3.x ciblait EXILED 3.0.0-alpha.83. La reecriture corrige plusieurs
defauts de comportement du plugin d'origine :

- **Priorite des operateurs.** Le test de position s'ecrivait
  `IsKeycard() || IsMedical() || IsUtility() || IsScp() && position != zero`.
  `&&` liant plus fort que `||`, la verification de coordonnee ne s'appliquait
  qu'aux objets SCP : les cartes, soins et utilitaires etaient envoyes a
  `0,0,0` quand la position n'etait pas configuree. Meme defaut sur les armes.
- **Fuite de position entre objets.** La variable de position etait declaree
  hors de la boucle : un objet ne correspondant a aucune categorie heritait de
  la position de l'objet precedent au lieu du point de repli.
- **Tirage decale.** `Random.Next(100) <= Chance` donnait 1 % de chance a un
  objet configure a `Chance = 0`.
- **Objet reutilise.** Une seule instance d'objet etait creee puis « spawnee »
  autant de fois que la quantite demandee, au lieu d'un pickup par exemplaire.
- **Verification des joueurs au mauvais moment.** Le nombre de joueurs etait
  teste *avant* l'attente de dix minutes, pas au moment du largage.
- **Intervalle du vehicule fausse.** `TimeDifference` etait attendu a chaque
  iteration de la boucle et non une seule fois, donc l'intervalle reel valait
  `TimeDifference + CarTime + 30` au lieu de `CarTime`.
- **Alias de config.** `using Config = Exiled.Loader.Config;` masquait la classe
  de configuration du plugin dans `Plugin.cs`.
- **Nettoyage incomplet.** Les coroutines n'etaient arretees qu'a
  `WaitingForPlayers`, pas a `RoundEnded` ni `RestartingRound`.

`RespawnEffectsController`, qui jouait le son d'arrivee, a disparu avec la
refonte des vagues de respawn de la 14.0. Il est remplace par une annonce
C.A.S.S.I.E configurable par profil.

Les deux coroutines independantes sont remplacees par **un seul ordonnanceur**
qui pilote tous les profils, conformement aux conventions du projet.
