// Copyright (c) Microsoft Corporation. All rights reserved. 
// Licensed under the MIT License. See License.txt in the project root for license information. 

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Azure.Identity;
using System.Windows.Forms;
using Microsoft.Vault.Library;
using Azure.Core;

namespace Microsoft.Vault.Explorer
{
    public partial class SubscriptionsManagerDialog : Form
    {
        static string ApiVersion => $"api-version={AppConfig.Current.Azure.SubscriptionApiVersion}";
        static string ManagmentEndpoint => AppConfig.Current.Azure.ManagementEndpoint;
        static string ManagementScope => AppConfig.Current.Azure.ManagementScope;
        const string AddAccountText = "Add New Account";
        const string ClientId = "Set ClientId here...";
        const string AddDomainHintText = "How to add new domain hint here...";
        const string AddDomainHintInstructions = @"To add new domain hint, just follow below steps:
1) In the main window open Settings dialog
2) Add domain hint line to 'Domain hints' property
3) Click on 'OK' button to save and close Settings dialog
4) Open Subscriptions Manager dialog";

        private AccountItem _currentAccountItem;
        private InteractiveBrowserCredential _credential;
        private readonly HttpClient _httpClient;

        public VaultAlias CurrentVaultAlias { get; private set; }

        public SubscriptionsManagerDialog()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            Font = new System.Drawing.Font("Segoe UI", 9.75F);

            // Create Default accounts based on domain hints and aliases.
            foreach (string userAccountName in Settings.Default.UserAccountNamesList)
            {
                string[] accounts = userAccountName.Split('@');
                uxComboBoxAccounts.Items.Add(new AccountItem(accounts[1], accounts[0]));
            }
            uxComboBoxAccounts.Items.Add(AddAccountText);
            uxComboBoxAccounts.Items.Add(AddDomainHintText);
            uxComboBoxAccounts.SelectedIndex = 0;
        }

        private UxOperation NewUxOperationWithProgress(params ToolStripItem[] controlsToToggle) => new UxOperation(null, uxStatusLabel, uxProgressBar, uxButtonCancelOperation, controlsToToggle);

        private async void uxComboBoxAccounts_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                switch (uxComboBoxAccounts.SelectedItem)
                {
                    case null:
                        return;

                    case AddAccountText:
                        AddNewAccount();
                        break;

                    case AddDomainHintText:
                        // Display instructions on how to add domain hint
                        MessageBox.Show(AddDomainHintInstructions, Utils.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        uxComboBoxAccounts.SelectedItem = null;
                        return;

                    case AccountItem account:
                        // Authenticate into selected account
                        _currentAccountItem = account;
                        GetAuthenticationToken();
                        if (_credential == null)
                        {
                            MessageBox.Show("Failed to authenticate selected account.", Utils.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        var authRecord = _credential.Authenticate();
                        _currentAccountItem.AuthRecord = authRecord;
                        _currentAccountItem.UserAlias = authRecord.Username;
                        break;

                    default:
                        return;
                }

                using (var op = NewUxOperationWithProgress(uxComboBoxAccounts))
                {
                    var token = _credential.GetToken(new TokenRequestContext(new[] { ManagementScope }));
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

                    var hrm = await _httpClient.GetAsync($"{ManagmentEndpoint}subscriptions?{ApiVersion}", op.CancellationToken);
                    var json = await hrm.Content.ReadAsStringAsync();
                    if (!hrm.IsSuccessStatusCode)
                    {
                        MessageBox.Show($"Could not load your Azure subscriptions.\n\nThis may be due to:\n\u2022 Insufficient permissions on your account\n\u2022 Network connectivity issues\n\u2022 Azure service availability\n\nHTTP {(int)hrm.StatusCode}: {hrm.ReasonPhrase}", Utils.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var subs = JsonConvert.DeserializeObject<SubscriptionsResponse>(json);

                    uxListViewSubscriptions.Items.Clear();
                    uxListViewVaults.Items.Clear();
                    uxPropertyGridVault.SelectedObject = null;
                    foreach (var s in subs?.Subscriptions ?? Array.Empty<Subscription>())
                    {
                        uxListViewSubscriptions.Items.Add(new ListViewItemSubscription(s));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not connect to Azure.\n\nPlease verify your network connection and try again.\n\nDetails: {ex.Message}", Utils.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void uxListViewSubscriptions_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListViewItemSubscription s = uxListViewSubscriptions.SelectedItems.Count > 0 ? (ListViewItemSubscription)uxListViewSubscriptions.SelectedItems[0] : null;
            if (null == s) return;
            using (var op = NewUxOperationWithProgress(uxComboBoxAccounts))
            {
                var token = _credential.GetToken(new TokenRequestContext(new[] { ManagementScope }));
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
                var subscriptionId = s.Subscription.SubscriptionId.ToString();
                var hrm = await _httpClient.GetAsync($"{ManagmentEndpoint}subscriptions/{subscriptionId}/providers/Microsoft.KeyVault/vaults?api-version={AppConfig.Current.Azure.KeyVaultApiVersion}", op.CancellationToken);
                var json = await hrm.Content.ReadAsStringAsync();
                var vaultsResponse = JsonConvert.DeserializeObject<VaultsListResponse>(json);
                uxListViewVaults.Items.Clear();
                if (vaultsResponse?.Value != null)
                {
                    foreach (var v in vaultsResponse.Value)
                    {
                        uxListViewVaults.Items.Add(new ListViewItemVault(v));
                    }
                }
            }
        }

        private async void uxListViewVaults_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListViewItemSubscription s = uxListViewSubscriptions.SelectedItems.Count > 0 ? (ListViewItemSubscription)uxListViewSubscriptions.SelectedItems[0] : null;
            ListViewItemVault v = uxListViewVaults.SelectedItems.Count > 0 ? (ListViewItemVault)uxListViewVaults.SelectedItems[0] : null;
            uxButtonOK.Enabled = false;
            if ((null == s) || (null == v)) return;
            using (var op = NewUxOperationWithProgress(uxComboBoxAccounts))
            {
                var subscriptionId = s.Subscription.SubscriptionId.ToString();
                var hrm = await _httpClient.GetAsync($"{ManagmentEndpoint}subscriptions/{subscriptionId}/resourceGroups/{v.GroupName}/providers/Microsoft.KeyVault/vaults/{v.Name}?api-version={AppConfig.Current.Azure.KeyVaultApiVersion}", op.CancellationToken);
                var json = await hrm.Content.ReadAsStringAsync();
                var vault = JsonConvert.DeserializeObject<VaultResource>(json);
                uxPropertyGridVault.SelectedObject = new PropertyObjectVault(s.Subscription, v.GroupName, vault!);
                uxButtonOK.Enabled = true;
                CurrentVaultAlias = new VaultAlias(v.Name, new string[] { v.Name }, new string[] { "Custom" }) { DomainHint = _currentAccountItem.DomainHint, UserAlias = _currentAccountItem.UserAlias};
            }
        }

        private void AddNewAccount()
        {
            // Create temp account item for new account
            _currentAccountItem = new AccountItem(Guid.NewGuid().ToString());
            GetAuthenticationToken();

            // Get new user account and add it to default settings
            string userAccountName = _credential.Authenticate().Username;
            AuthenticationRecord authentication = _credential.Authenticate();
            string[] userLogin = userAccountName.Split('@');
            _currentAccountItem.UserAlias = userLogin[0];
            _currentAccountItem.DomainHint = userLogin[1];
            Settings.Default.AddUserAccountName(userAccountName);

            // Rename cache to be associated with user login
            (_currentAccountItem.CachePersistence)?.Rename(userAccountName, authentication);
            uxComboBoxAccounts.Items.Insert(0, userAccountName);
            uxComboBoxAccounts.SelectedIndex = 0;
        }

        // Attempt to authenticate with current account.
        private void GetAuthenticationToken()
        {
            VaultAccessUserInteractive vaui = new VaultAccessUserInteractive(_currentAccountItem.DomainHint, _currentAccountItem.UserAlias);
            _credential = vaui.AcquireToken(_currentAccountItem.AuthRecord, _currentAccountItem.UserAlias);
        }
    }

    #region Aux UI related classes

    internal sealed class AccountItem
    {
        public AuthenticationRecord? AuthRecord { get; set; }
        public CachePersistence? CachePersistence { get; set; }

        public string DomainHint { get; set; }
        public string UserAlias { get; set; }
        private static readonly object FileLock = new object();
        public static readonly string FileName = Environment.ExpandEnvironmentVariables(string.Format(Consts.VaultTokenCacheFileName, "microsoft.com"));

        public AccountItem(string domainHint, string userAlias = null)
        {
            DomainHint = domainHint;
            UserAlias = userAlias ?? Environment.UserName;
            CachePersistence = new CachePersistence(this.ToString());
        }

        public override string ToString() => $"{UserAlias}@{DomainHint}";
    }

    internal sealed class ListViewItemSubscription : ListViewItem
    {
        public Subscription Subscription { get; }

        public ListViewItemSubscription(Subscription s) : base(s.DisplayName)
        {
            Subscription = s;
            Name = s.DisplayName;
            SubItems.Add(s.SubscriptionId.ToString());
            ToolTipText = $"State: {s.State}";
            ImageIndex = 0;
        }
    }

    internal sealed class ListViewItemVault : ListViewItem
    {
        private static readonly Regex s_resourceNameRegex = new Regex(@".*\/resourceGroups\/(?<GroupName>[a-zA-Z0-9_\-\.]{1,64})\/", RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public VaultResource Vault { get; }
        public string GroupName { get; }

        public ListViewItemVault(VaultResource vault) : base(vault.Name)
        {
            Vault = vault;
            Name = vault.Name;
            GroupName = s_resourceNameRegex.Match(vault.Id ?? "").Groups["GroupName"].Value;
            SubItems.Add(GroupName);
            ToolTipText = $"Location: {vault.Location}";
            ImageIndex = 1;
        }
    }

    public class PropertyObjectVault
    {
        private readonly Subscription _subscription;
        private readonly string _resourceGroupName;
        private readonly VaultResource _vault;

        public PropertyObjectVault(Subscription s, string resourceGroupName, VaultResource vault)
        {
            _subscription = s;
            _resourceGroupName = resourceGroupName;
            _vault = vault;
            Tags = new ObservableTagItemsCollection();
            if (null != _vault.Tags) foreach (var kvp in _vault.Tags) Tags.Add(new TagItem(kvp));
            AccessPolicies = new ObservableAccessPoliciesCollection();
            int i = -1;
            if (_vault.Properties?.AccessPolicies != null)
                foreach (var ape in _vault.Properties.AccessPolicies) AccessPolicies.Add(new AccessPolicyEntryItem(++i, ape));
        }

        [DisplayName("Name")]
        [ReadOnly(true)]
        public string? Name => _vault.Name;

        [DisplayName("Location")]
        [ReadOnly(true)]
        public string? Location => _vault.Location;

        [DisplayName("Uri")]
        [ReadOnly(true)]
        public string? Uri => _vault.Properties?.VaultUri;

        [DisplayName("Subscription Name")]
        [ReadOnly(true)]
        public string DisplayName => _subscription.DisplayName;

        [DisplayName("Subscription Id")]
        [ReadOnly(true)]
        public Guid SubscriptionId => _subscription.SubscriptionId;

        [DisplayName("Resource Group Name")]
        [ReadOnly(true)]
        public string ResourceGroupName => _resourceGroupName;

        [DisplayName("Custom Tags")]
        [ReadOnly(true)]
        public ObservableTagItemsCollection Tags { get; private set; }

        [DisplayName("Sku")]
        [ReadOnly(true)]
        public string? Sku => _vault.Properties?.Sku?.Name;

        [DisplayName("Access Policies")]
        [ReadOnly(true)]
        [TypeConverter(typeof(ExpandableCollectionObjectConverter))]
        public ObservableAccessPoliciesCollection AccessPolicies { get; }
    }

    [Editor(typeof(ExpandableCollectionEditor<ObservableAccessPoliciesCollection, AccessPolicyEntryItem>), typeof(UITypeEditor))]
    public class ObservableAccessPoliciesCollection : ObservableCustomCollection<AccessPolicyEntryItem>
    {
        public ObservableAccessPoliciesCollection() : base() { }

        public ObservableAccessPoliciesCollection(IEnumerable<AccessPolicyEntryItem> collection) : base(collection) { }

        protected override PropertyDescriptor GetPropertyDescriptor(AccessPolicyEntryItem item) =>
            new ReadOnlyPropertyDescriptor($"[{item.Index}]", item);
    }

    [Editor(typeof(ExpandableObjectConverter), typeof(UITypeEditor))]
    public class AccessPolicyEntryItem
    {
        private static string[] EmptyList = new string[] { };
        private AccessPolicyEntry _ape;

        public AccessPolicyEntryItem(int index, AccessPolicyEntry ape)
        {
            Index = index;
            _ape = ape;
        }

        [JsonIgnore]
        public int Index { get; }

        [Description("Application ID of the client making request on behalf of a principal")]
        public Guid? ApplicationId => _ape.ApplicationId != null ? Guid.Parse(_ape.ApplicationId) : null;

        [Description("Object ID of the principal")]
        public string? ObjectId => _ape.ObjectId;

        [Description("Permissions to keys")]
        public string PermissionsToKeys => string.Join(",", _ape.Permissions?.Keys ?? EmptyList);

        [Description("Permissions to secrets")]
        public string PermissionsToSecrets => string.Join(",", _ape.Permissions?.Secrets ?? EmptyList);

        [Description("Permissions to certificates")]
        public string PermissionsToCertificates => string.Join(",", _ape.Permissions?.Certificates ?? EmptyList);

        [Description("Tenant ID of the principal")]
        public string? TenantId => _ape.TenantId;

        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
    }

    #endregion

    #region Azure REST API DTO classes

    [JsonObject]
    public class VaultsListResponse
    {
        [JsonProperty(PropertyName = "value")]
        public VaultResource[]? Value { get; set; }
    }

    [JsonObject]
    public class VaultResource
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Location { get; set; }
        public string? Type { get; set; }
        public Dictionary<string, string>? Tags { get; set; }
        public VaultProperties? Properties { get; set; }
    }

    [JsonObject]
    public class VaultProperties
    {
        public string? VaultUri { get; set; }
        public VaultSku? Sku { get; set; }
        public AccessPolicyEntry[]? AccessPolicies { get; set; }
    }

    [JsonObject]
    public class VaultSku
    {
        public string? Name { get; set; }
    }

    [JsonObject]
    public class AccessPolicyEntry
    {
        public string? TenantId { get; set; }
        public string? ObjectId { get; set; }
        public string? ApplicationId { get; set; }
        public AccessPolicyPermissions? Permissions { get; set; }
    }

    [JsonObject]
    public class AccessPolicyPermissions
    {
        public string[]? Keys { get; set; }
        public string[]? Secrets { get; set; }
        public string[]? Certificates { get; set; }
    }

    #endregion

    #region Managment endpoint JSON response classes

    [JsonObject]
    public class SubscriptionsResponse
    {
        [JsonProperty(PropertyName = "value")]
        public Subscription[] Subscriptions { get; set; }
    }

    [JsonObject]
    public class Subscription
    {
        public string Id { get; set; }
        public Guid SubscriptionId { get; set; }
        public string DisplayName { get; set; }
        public string State { get; set; }
        public string AuthorizationSource { get; set; }
    }

    #endregion
}
