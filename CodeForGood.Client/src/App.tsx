import { useEffect, useState } from 'react'
import './App.css'

interface WeatherForecast {
  date: string
  temperatureC: number
  temperatureF: number
  summary: string | null
}

function App() {
  const [forecasts, setForecasts] = useState<WeatherForecast[] | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    fetch('/weatherforecast')
      .then((res) => {
        if (!res.ok) throw new Error(`HTTP ${res.status}`)
        return res.json()
      })
      .then(setForecasts)
      .catch((err) => setError(String(err)))
  }, [])

  if (error) {
    return <p>{error}</p>
  }

  if (forecasts === null) {
    return <p>Loading…</p>
  }

  return (
    <main id="center">
      <h1>Weather forecast</h1>
      <table>
        <thead>
          <tr>
            <th>Date</th>
            <th>Temp. (°C)</th>
            <th>Temp. (°F)</th>
            <th>Summary</th>
          </tr>
        </thead>
        <tbody>
          {forecasts.map((f) => (
            <tr key={f.date}>
              <td>{f.date}</td>
              <td>{f.temperatureC}</td>
              <td>{f.temperatureF}</td>
              <td>{f.summary}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </main>
  )
}

export default App
