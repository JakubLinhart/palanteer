namespace Palanteer.Desktop
{
    public sealed class PlaceMarker : Marker
    {
        public PlaceMarker(Place place, bool canEdit = true) : base(place, canEdit)
        {
            Place = place;
            Name = place.Name;
            X = place.X;
            Y = place.Y;
        }

        public Place Place { get; }
    }
}