# SupplyDrop 4.0

> Portage EXILED 9.14.2 d'un plugin de **Wafel, KadeDev, JesusQC**. Depot non
> affilie aux auteurs d'origine. Voir [NOTICE.md](NOTICE.md) pour l'attribution.

Largages de ravitaillement periodiques pour le Foudroyeur et l'Insurrection du Chaos.

**EXILED 9.14.2** — `dotnet build -c Release SupplyDrop/SupplyDrop.csproj`

## Profils

Les largages sont **pilotes par les donnees**. Chaque entree de `profiles` decrit
un type de largage complet : contenu, horaires, positions, annonce, balise.
Ajouter une entree avec une cle inutilisee cree un nouveau type de largage sans
recompiler.

Deux profils par defaut : `mtf` et `chaos`.

| Cle | Role |
|---|---|
| `first_drop_seconds` | Delai avant le premier largage, depuis le debut du round |
| `interval_seconds` | Delai entre deux largages du meme profil |
| `announcement_lead_seconds` | Temps entre l'annonce et l'apparition des objets |
| `max_drops_per_round` | Plafond par round. `-1` = illimite |
| `ammo/armor/item/weapon_position` | Coordonnees par categorie. `0,0,0` = spawn du role de repli |
| `scatter_radius` | Dispersion horizontale autour du point, en metres |
| `enable_beacon` | Halo lumineux sur la zone de largage |
| `cassie_announcement` | Annonce C.A.S.S.I.E. Vide = desactivee |

## Commandes

| Commande | Permission | Effet |
|---|---|---|
| `supplydrop list` | `supplydrop.call` | Liste les profils, leurs horaires et leur etat |
| `supplydrop call <profil>` | `supplydrop.call` | Declenche un largage immediatement, avec annonce |
| `supplydrop call <profil> silencieux` | `supplydrop.call` | Largage sans annonce ni C.A.S.S.I.E |

Alias `sd`. Chaque appel est trace avec l'auteur.

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
