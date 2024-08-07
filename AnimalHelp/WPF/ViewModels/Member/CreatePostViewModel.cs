﻿using AnimalHelp.Application.Services.AdoptionCentre;
using AnimalHelp.Application.Services.Post;
using AnimalHelp.Application.Stores;
using AnimalHelp.Application.UseCases.User;
using AnimalHelp.Application.Utility.Navigation;
using AnimalHelp.Domain.Model;
using AnimalHelp.WPF.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using ValidationError = AnimalHelp.Domain.Model.ValidationError;

namespace AnimalHelp.WPF.ViewModels.Member
{
    public class CreatePostViewModel : ViewModelBase, INavigableDataContext
    {
        private string? _description;
        private List<Photo> _photos;
        private Animal _animal;
        private string? _photoDescription;
        private string? _photoURL;

        private string? _errorDescriptionRequired;
        private string? _errorPhotos;
        private string? _errorAnimalRequired;

        public RelayCommand AddPostCommand { get; }
        public RelayCommand UpdatePostCommand { get; }

        public RelayCommand AddPhotoCommand { get; }
        public RelayCommand RemovePhotoCommand { get; }


        private readonly IMemberService _memberService;
        private readonly IPostService _postService;
        private readonly IAnimalService _animalService;
        private readonly INavigationService _navigationService;
        private readonly IAuthenticationStore _authenticationStore;

        private ObservableCollection<Post> _posts;
        public ObservableCollection<Post> Posts
        {
            get => _posts;
            set
            {
                _posts = value;
                OnPropertyChanged(nameof(Posts));
            }
        }

        public ObservableCollection<Animal> Animals { get; set; }

        private ObservableCollection<Photo> _chosenPhotos;
        public ObservableCollection<Photo> ChosenPhotos
        {
            get => _chosenPhotos;
            set
            {
                _chosenPhotos = value;
                OnPropertyChanged(nameof(ChosenPhotos));
            }
        }

        public bool selectingPost;
        private Post? _selectedItem;
        public Post? SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                selectingPost = true;
                if (_selectedItem != null)
                {
                    Domain.Model.Post? post = _postService.GetById(_selectedItem.Id);
                    if (post == null)
                        return;
                    Description = post.Description;
                    if (post.Photos != null)
                    {
                        ChosenPhotos = [.. post.Photos];
                    }
                    Animal = post.Animal;
                }
                selectingPost = false;
                OnPropertyChanged();
            }
        }

        private Animal _selectedAnimal;
        public Animal SelectedAnimal
        {
            get => _selectedAnimal;
            set => SetField(ref _selectedAnimal, value);
        }

        public NavigationStore NavigationStore { get; }


        public CreatePostViewModel(IAuthenticationStore authenticationStore, IAnimalService animalService, IMemberService memberService, IPostService postService, INavigationService navigationService, NavigationStore navigationStore)
        {
            _authenticationStore = authenticationStore;
            _memberService = memberService;
            _postService = postService;
            _animalService = animalService;
            _navigationService = navigationService;
            NavigationStore = navigationStore;
            
            Posts = new();
            Animals = new();
            ChosenPhotos = new();

            LoadCollections();
            AddPostCommand = new RelayCommand(AddPost!);
            UpdatePostCommand = new RelayCommand(UpdatePost!);
            AddPhotoCommand = new RelayCommand(AddPhoto!);
            RemovePhotoCommand = new RelayCommand(RemovePhoto!);
        }

        public void LoadCollections()
        {
            LoadPosts();
            LoadAnimals();
        }
        private void LoadPosts()
        {
            Posts.Clear();
            List<Domain.Model.Post> posts;

            posts = _postService.GetAll();
            foreach (Domain.Model.Post post in posts)
            {
                if(post.Status == PostStatus.Approved || post.Status == PostStatus.PendingUpdate)
                {
                    Posts.Add(post);
                }
            }
        }

        private void LoadAnimals()
        {
            Animals.Clear();
            List<Domain.Model.Animal> animals;

            animals = _animalService.GetAnimalsWithoutPost();
            foreach (Domain.Model.Animal animal in animals)
                Animals.Add(animal);
        }

        public string? ErrorDescriptionRequired
        {
            get => _errorDescriptionRequired;
            set => SetField(ref _errorDescriptionRequired, value);
        }

        public string? ErrorPhotos
        {
            get => _errorPhotos;
            set => SetField(ref _errorPhotos, value);
        }

        public string? ErrorAnimalRequired
        {
            get => _errorAnimalRequired;
            set => SetField(ref _errorAnimalRequired, value);
        }

        public string? Description
        {
            get => _description;
            set => SetField(ref _description, value);
        }

        public List<Photo>? Photos
        {
            get => _photos;
            set => SetField(ref _photos, value);
        }

        public string? PhotoDescription
        {
            get => _photoDescription;
            set => SetField(ref _photoDescription, value);
        }

        private PostState? _postState;
        public PostState? PostState
        {
            get => _postState;
            set => SetField(ref _postState, value);
        }

        public string? PhotoURL
        {
            get => _photoURL;
            set => SetField(ref _photoURL, value);
        }

        public Animal? Animal
        {
            get => _animal;
            set => SetField(ref _animal, value);
        }
        private Photo _selectedPhoto;

        public Photo SelectedPhoto
        {
            get => _selectedPhoto;
            set
            {
                _selectedPhoto = value;
                OnPropertyChanged(nameof(SelectedPhoto));
            }
        }
        private void AddPost(object parameter)
        {
            ErrorDescriptionRequired = "";
            ErrorPhotos = "";
            ErrorAnimalRequired = "";

            string? description = Description;
            List<Photo>? photos = ChosenPhotos.ToList();
            Animal? animal = SelectedAnimal;

            if(description == null || animal == null)
            {
                return;
            }

            Post newPost = new Post(Description, photos, animal);

            if(_authenticationStore.CurrentUser.UserType == UserType.Member) 
            {
                newPost.Status = PostStatus.PendingApproval;
            }
            else if(_authenticationStore.CurrentUser.UserType == UserType.Volunteer)
            {
                newPost.Status = PostStatus.Approved;
                Posts.Add(newPost);
            }

            _postService.Add(newPost);
            RemoveInputs();
        }

        private void UpdatePost(object parameter)
        {
            if (SelectedItem == null)
                return;

            Domain.Model.Post post = new Post(SelectedItem.Id, Description, ChosenPhotos.ToList(), SelectedItem.Animal);

            if (_authenticationStore.CurrentUser.UserType == UserType.Member)
            {
                post.Status = PostStatus.PendingUpdate;
                _postService.Update(SelectedItem.Id, post);
            }
            else if (_authenticationStore.CurrentUser.UserType == UserType.Volunteer)
            {
                post.Status = PostStatus.Approved;
                _postService.Update(SelectedItem.Id, post);
                Posts.Remove(SelectedItem);
                Posts.Add(post);
            }
    
            RemoveInputs();
        }

        private void RemoveInputs()
        {
            Description = "";
            PhotoDescription = "";
            PhotoURL = "";
            ChosenPhotos.Clear();

            selectingPost = true;
        }

        private void AddPhoto(object parameter)
        {
            string? photoDescription = PhotoDescription;
            string? photoURL = PhotoURL;

            if(photoDescription == null || photoURL == null)
            {
                return;
            }

            Photo photo = new Photo(photoURL, photoDescription);
            ChosenPhotos.Add(photo);
        }
        
        private void RemovePhoto(object parameter)
        {
            if (SelectedPhoto != null)
            {
                ChosenPhotos.Remove(SelectedPhoto);
            }
        }
    }
}
