﻿using Caliburn.Micro;
using IO.Addons.Controller;
using IO.Addons.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WPFUI.ViewModels
{
    public class AddonsViewModel : Screen
    {
        private AddonsControllerFactory addonControllerFactory = new AddonsControllerFactory();
        private IAddonController addonController;

        private BindableCollection<IAddonInfo> _addons;
        private string _searchTerm;

        //The folder all addons are currently loaded from. Saved in case user changes the folder during runtime. (then we reload the addons). 
        private string currentAddonFolder;

        public ICommand RemoveAddonsCommand { get; set; }

        public bool ShowSettingsPrompt
        {
            get
            {
                return string.IsNullOrWhiteSpace(Properties.Settings.Default.AddonFolder);
            }
        }

        public object SettingsPrompt { get; set; }

        public BindableCollection<IAddonInfo> Addons
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(SearchTerm))
                    return new BindableCollection<IAddonInfo>(_addons.Where(addon => addon.Title.ToLower().Contains(SearchTerm.ToLower())));

                return _addons;
            }
            set
            {
                _addons = value;
                NotifyOfPropertyChange(() => Addons);
            }
        }

        public string SearchTerm
        {
            get { return _searchTerm; }
            set
            {
                _searchTerm = value;
                NotifyOfPropertyChange(() => Addons);
            }
        }

        public AddonsViewModel()
        {
            RemoveAddonsCommand = new Command(RemoveAddons);
            addonController = addonControllerFactory.CreateAddonController();

            SettingsPrompt = new Views.SettingsPrompt();
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            if ((!ShowSettingsPrompt && Addons == null) || Properties.Settings.Default.AddonFolder != currentAddonFolder)
                LoadAddons();
        }

        public void LoadAddons()
        {
            try
            {
                var addons = addonController.GetAddons(Properties.Settings.Default.AddonFolder);
                Addons = new BindableCollection<IAddonInfo>(addons);

                currentAddonFolder = Properties.Settings.Default.AddonFolder;
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show($"\"{Properties.Settings.Default.AddonFolder}\" is not a valid World of Warcraft root folder.");
            }
        }

        public void RemoveAddons(object selectedAddons)
        {

            IEnumerable<IAddonInfo> addonsCollection = (selectedAddons as IEnumerable<object>).Select(o => o as IAddonInfo);

            if (!addonsCollection.Any())
                return;

            string addonNames = string.Join(",\n", addonsCollection.Select(o => o.Title));

            System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show($"Are you sure you want to remove the following Addons: \n{addonNames}", "Confirm addon deletion", System.Windows.MessageBoxButton.YesNo);

            if(result == System.Windows.MessageBoxResult.Yes)
            {
                addonController.RemoveAddons(addonsCollection);
                Addons.RemoveRange(addonsCollection);
            }


        }
    }
}
