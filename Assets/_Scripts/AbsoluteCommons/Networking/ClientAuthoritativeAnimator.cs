using Unity.Netcode.Components;
using UnityEngine;

namespace AbsoluteCommons.Networking {
	[AddComponentMenu("Absolute Commons/Networking/Client Authoritative Animator")]
	[RequireComponent(typeof(Animator))]
	public class ClientAuthoritativeAnimator : NetworkAnimator {
		protected override bool OnIsServerAuthoritative() => false;
	}
}
