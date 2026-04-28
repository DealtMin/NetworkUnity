using Unity.Netcode.Components;
using UnityEngine;

namespace Unity.Netcode.Components
{
    [DisallowMultipleComponent]
    public class ClientNetworkTransform : NetworkTransform
    {
        protected override bool OnIsServerAuthoritative()
        {
            return false; // Клиент имеет авторитет над своим движением
        }
    }
}