using System.Collections.Generic;
using System.Drawing;


namespace Composer.Editor
{
    class TrackSegmentPitchedNotes : TrackSegment
    {
        public Util.Pitch minPitch, maxPitch;
        public List<Project.TrackPitchedNotes> projectTracks;

        const float LAYOUT_MARGIN = 4;


        public TrackSegmentPitchedNotes(
            ViewManager manager,
            Row row,
            List<Project.TrackPitchedNotes> projectTracks)
            : base(manager, row)
        {
            this.projectTracks = projectTracks;
        }


        public override void Rebuild(float x, float y)
        {
            this.minPitch = Util.Pitch.FromMidiPitch(60);
            this.maxPitch = Util.Pitch.FromMidiPitch(71);

            // Find minimum and maximum note pitches in the segment's time range.
            foreach (var track in this.projectTracks)
            {
                foreach (var note in track.notes.EnumerateOverlappingRange(this.row.timeRange))
                {
                    var noteOctave = (int)(note.pitch.MidiPitch / 12);

                    if (12 * noteOctave < this.minPitch.MidiPitch)
                        this.minPitch.MidiPitch = 12 * noteOctave;

                    if (12 * noteOctave + 11 > this.maxPitch.MidiPitch)
                        this.maxPitch.MidiPitch = 12 * noteOctave + 11;
                }
            }

            var pitchRange = (this.maxPitch.MidiPitch - this.minPitch.MidiPitch + 1);

            this.layoutRect = new Util.Rect(
                x,
                y,
                x + this.row.timeRange.Duration * this.manager.TimeToPixelsMultiplier,
                y + LAYOUT_MARGIN * 2 + pitchRange * this.manager.PitchedNoteHeight);

            this.contentRect = new Util.Rect(
                x,
                y + LAYOUT_MARGIN,
                x + this.row.timeRange.Duration * this.manager.TimeToPixelsMultiplier,
                y + LAYOUT_MARGIN + pitchRange * this.manager.PitchedNoteHeight);
        }


        public override float GetTimeAtPosition(float x)
        {
            return this.row.timeRange.Start + (x - this.contentRect.xMin) / this.manager.TimeToPixelsMultiplier;
        }


        public override Util.Pitch GetPitchAtPosition(float y)
        {
            return Util.Pitch.FromMidiPitch(
                System.Math.Max(this.minPitch.MidiPitch,
                System.Math.Min(this.maxPitch.MidiPitch,
                    this.minPitch.MidiPitch + (this.contentRect.yMax - y) / this.manager.PitchedNoteHeight)));
        }


        public override void Draw(Graphics g)
        {
            var rowEndTime = System.Math.Max(this.row.timeRange.End, this.row.resizeEndTime);
            var rowDuration = rowEndTime - this.row.timeRange.Start;
            var rowEndX = (int)(this.layoutRect.xMin + rowDuration * this.manager.TimeToPixelsMultiplier);

            // Draw beat separators.
            for (var i = 0; i < this.row.trackSegmentMeterChanges.affectingMeterChanges.Count; i++)
            {
                var meterChange = this.row.trackSegmentMeterChanges.affectingMeterChanges[i];
                if (meterChange == null)
                    continue;

                var meterEndTime = rowEndTime;
                if (i + 1 < this.row.trackSegmentMeterChanges.affectingMeterChanges.Count)
                    meterEndTime = this.row.trackSegmentMeterChanges.affectingMeterChanges[i + 1].time;

                var beatCount = 0;
                var beatDuration = this.manager.project.WholeNoteDuration / meterChange.meter.denominator;

                for (var n = meterChange.time; n < meterEndTime; n += beatDuration)
                {
                    if (n > this.row.timeRange.Start)
                    {
                        var nMinusRowStart = n - this.row.timeRange.Start;
                        var x = (int)this.contentRect.xMin + nMinusRowStart * this.manager.TimeToPixelsMultiplier;

                        g.DrawLine(beatCount == 0 ? Pens.Gray : Pens.LightGray,
                            x,
                            (int)(this.contentRect.yMin),
                            x,
                            (int)(this.contentRect.yMax));
                    }

                    beatCount = (beatCount + 1) % meterChange.meter.numerator;
                }
            }

            using (var font = new Font("Verdana", this.manager.PitchedNoteHeight / 1.5f))
            {
                // Draw pitch separators.
                for (var i = 0; i < this.row.trackSegmentKeyChanges.affectingKeyChanges.Count; i++)
                {
                    var keyChange = this.row.trackSegmentKeyChanges.affectingKeyChanges[i];

                    var keyStartTime = this.row.timeRange.Start;
                    if (keyChange != null)
                        keyStartTime = System.Math.Max(keyStartTime, keyChange.time);

                    var keyEndTime = rowEndTime;
                    if (i + 1 < this.row.trackSegmentKeyChanges.affectingKeyChanges.Count)
                        keyEndTime = this.row.trackSegmentKeyChanges.affectingKeyChanges[i + 1].time;

                    var keyStartX = (int)
                        (this.contentRect.xMin + (keyStartTime - this.row.timeRange.Start) *
                        this.manager.TimeToPixelsMultiplier);

                    var keyEndX = (int)
                        (this.contentRect.xMin + (keyEndTime - this.row.timeRange.Start) *
                        this.manager.TimeToPixelsMultiplier);

                    for (var p = this.minPitch.MidiPitch; p <= this.maxPitch.MidiPitch; p++)
                    {
                        var pitch = Util.Pitch.FromMidiPitch(p);
                        var relativePitch = Util.RelativePitchData.MakeFromPitch(pitch);

                        var isTonicPitch =
                            keyChange != null &&
                            relativePitch == keyChange.key.tonicPitch;

                        var y = (int)
                            (this.contentRect.yMax - (p - this.minPitch.MidiPitch) *
                            this.manager.PitchedNoteHeight);

                        g.DrawLine(isTonicPitch ? Pens.Gray : Pens.LightGray,
                            keyStartX, y,
                            keyEndX, y);

                        var octave = (int)(pitch.MidiPitch / 12);

                        if (keyChange == null || keyChange.key.HasPitch(relativePitch))
                        {
                            g.DrawString(
                                Util.RelativePitchData.GetSimpleName(relativePitch),
                                font,
                                Brushes.MediumVioletRed,
                                keyStartX + 3, y - this.manager.PitchedNoteHeight);

                            g.DrawString(
                                octave.ToString(),
                                font,
                                Brushes.MediumVioletRed,
                                keyStartX + 15, y - this.manager.PitchedNoteHeight);
                        }
                    }
                }
            }

            // Draw frame.
            g.DrawRectangle(Pens.Black,
                (int)this.contentRect.xMin, (int)this.contentRect.yMin,
                rowEndX - (int)this.contentRect.xMin, (int)this.contentRect.ySize);

            g.DrawLine(Pens.Black,
                (int)this.contentRect.xMax - 1, (int)this.contentRect.yMin,
                (int)this.contentRect.xMax - 1, (int)this.contentRect.yMax);
        }
    }
}
